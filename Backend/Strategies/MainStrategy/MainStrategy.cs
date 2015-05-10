namespace Ezbob.Backend.Strategies.MainStrategy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using ConfigManager;
    using DbConstants;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
    using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval;
    using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject;
    using Ezbob.Backend.Strategies.Alibaba;
    using Ezbob.Backend.Strategies.Exceptions;
    using Ezbob.Backend.Strategies.Experian;
    using Ezbob.Backend.Strategies.MailStrategies.API;
    using Ezbob.Backend.Strategies.MedalCalculations;
    using Ezbob.Backend.Strategies.Misc;
    using Ezbob.Backend.Strategies.NewLoan;
    using Ezbob.Backend.Strategies.SalesForce;
    using Ezbob.Database;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using EZBob.DatabaseLib.Model.Database.Repository;
    using EZBob.DatabaseLib.Model.Database.UserManagement;
    using EZBob.DatabaseLib.Model.Loans;
    using EZBob.DatabaseLib.Repository;
    using EZBob.DatabaseLib.Repository.Turnover;
    using LandRegistryLib;
    using NHibernate;
    using SalesForceLib.Models;
    using StructureMap;

    public class MainStrategy : AStrategy {
        [DataContract]
        public enum DoAction {
            [EnumMember]
            Yes,

            [EnumMember]
            No,
        } // enum UpdateCashRequest

        public MainStrategy(
            int customerId,
            NewCreditLineOption newCreditLine,
            int avoidAutoDecision,
            FinishWizardArgs fwa,
            long? cashRequestID,
            DoAction createCashRequest,
            DoAction updateCashRequest
        ) {
            this.cashRequestID = cashRequestID;
            this.createCashRequest = createCashRequest;
            this.updateCashRequest = updateCashRequest;
            this.finishWizardArgs = fwa;

            this.session = ObjectFactory.GetInstance<ISession>();
            this.customers = ObjectFactory.GetInstance<CustomerRepository>();
            this.decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
            this.loanSourceRepository = ObjectFactory.GetInstance<LoanSourceRepository>();
            this.loanTypeRepository = ObjectFactory.GetInstance<LoanTypeRepository>();
            this.discountPlanRepository = ObjectFactory.GetInstance<DiscountPlanRepository>();
            this.customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
            this.landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();

            this.customerId = customerId;
            this.newCreditLineOption = newCreditLine;
            this.avoidAutomaticDecision = avoidAutoDecision;
            this.overrideApprovedRejected = true;

            this.autoDecisionResponse = new AutoDecisionResponse {
                DecisionName = "Manual",
            };
        } // constructor

        public override string Name {
            get { return "Main strategy"; }
        } // Name

        public override void Execute() {
            ValidateCashRequestArgs();

            if (this.finishWizardArgs != null)
                FinishWizard();

            if (this.newCreditLineOption == NewCreditLineOption.SkipEverything) {
                Log.Debug(
                    "Main strategy was activated in 'skip everything go to manual decision mode'." +
                    "Nothing more to do for customer id '{0}'. Bye.", this.customerId
                );

                return;
            } // if

            StrategiesMailer mailer = null;

            if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules) {
                mailer = new StrategiesMailer();

                var staller = new Staller(this.customerId, mailer);
                staller.Stall();

                ExecuteAdditionalStrategies();
            } // if

            this.customerDetails = new CustomerDetails(this.customerId);

            if (!this.customerDetails.IsTest) {
                var fraudChecker = new FraudChecker(this.customerId, FraudMode.FullCheck);
                fraudChecker.Execute();
            } // if

            ForceNhibernateResync();

            ProcessRejections();

            // Gather LR data - must be done after rejection decisions
            bool bSkip =
                this.newCreditLineOption == NewCreditLineOption.SkipEverything ||
                this.newCreditLineOption == NewCreditLineOption.SkipEverythingAndApplyAutoRules;

            if (!bSkip)
                GetLandRegistryDataIfNotRejected();

            var instance = new CalculateMedal(this.customerId, DateTime.UtcNow, false, true);
            instance.Execute();

            this.medal = instance.Result;

            CapOffer();

            ProcessApprovals();

            AdjustOfferredCreditLine();

            this.lastOffer = new LastOfferData(this.customerId);

            UpdateCustomerAndCashRequest();

            UpdateCustomerAnalyticsLocalData();

            SendEmails(mailer ?? new StrategiesMailer());
        } // Execute

        public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected) {
            this.overrideApprovedRejected = bOverrideApprovedRejected;
            return this;
        } // SetOverrideApprovedRejected

        public AutoDecisionResponse AutoDecisionResponse {
            get { return this.autoDecisionResponse; }
        } // AutoDecisionResponse

        private long? cashRequestID;
        private DoAction createCashRequest;
        private DoAction updateCashRequest;

        private void ForceNhibernateResync() {
            var customer = this.customers.ReallyTryGet(this.customerId);
            if (customer != null)
                this.session.Evict(customer);

            MarketplaceTurnoverRepository mpTurnoverRep = ObjectFactory.GetInstance<MarketplaceTurnoverRepository>();
            foreach (MarketplaceTurnover mpt in mpTurnoverRep.GetByCustomerId(this.customerId))
                if (mpt != null)
                    this.session.Evict(mpt);
        } // ForceNhibernateResync

        private void SendEmails(StrategiesMailer mailer) {
            bool sendToCustomer = true;
            Customer customer = this.customers.ReallyTryGet(this.customerId);

            if (customer != null) {
                int numOfPreviousApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);

                sendToCustomer = !customer.FilledByBroker || (numOfPreviousApprovals != 0);
            } // if

            var postMaster = new MainStrategyMails(
                mailer,
                this.customerId,
                this.offeredCreditLine,
                this.lastOffer,
                this.medal,
                this.customerDetails,
                this.autoDecisionResponse,
                sendToCustomer
            );

            postMaster.SendEmails();
        } // SendEmails

        private void UpdateCustomerAnalyticsLocalData() {
            SafeReader scoreCardResults = DB.GetFirst(
                "GetScoreCardData",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerId", this.customerId),
                new QueryParameter("Today", DateTime.Today)
            );

            int ezbobSeniorityMonths = scoreCardResults["EzbobSeniorityMonths"];

            int modelMaxFeedback = scoreCardResults["MaxFeedback", CurrentValues.Instance.DefaultFeedbackValue];

            int numOfEbayAmazonPayPalMps = scoreCardResults["MPsNumber"];
            int modelOnTimeLoans = scoreCardResults["OnTimeLoans"];
            int modelLatePayments = scoreCardResults["LatePayments"];
            int modelEarlyPayments = scoreCardResults["EarlyPayments"];

            bool firstRepaymentDatePassed = false;

            DateTime modelFirstRepaymentDate = scoreCardResults["FirstRepaymentDate"];
            if (modelFirstRepaymentDate != default(DateTime))
                firstRepaymentDatePassed = modelFirstRepaymentDate < DateTime.UtcNow;

            DB.ExecuteNonQuery(
                "CustomerAnalyticsUpdateLocalData",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerID", this.customerId),
                new QueryParameter("AnalyticsDate", DateTime.UtcNow),
                new QueryParameter("AnnualTurnover", this.medal.AnnualTurnover),
                new QueryParameter("TotalSumOfOrdersForLoanOffer", (decimal)0), // Not used any more, was part of old medal.
                new QueryParameter("MarketplaceSeniorityYears", (decimal)0), // Not used any more, was part of old medal.
                new QueryParameter("MaxFeedback", modelMaxFeedback),
                new QueryParameter("MPsNumber", numOfEbayAmazonPayPalMps),
                new QueryParameter("FirstRepaymentDatePassed", firstRepaymentDatePassed),
                new QueryParameter("EzbobSeniorityMonths", ezbobSeniorityMonths),
                new QueryParameter("OnTimeLoans", modelOnTimeLoans),
                new QueryParameter("LatePayments", modelLatePayments),
                new QueryParameter("EarlyPayments", modelEarlyPayments)
            );
        } // UpdateCustomerAnalyticsLocalData

        private void AdjustOfferredCreditLine() {
            if (this.autoDecisionResponse.IsAutoReApproval || this.autoDecisionResponse.IsAutoApproval)
                this.offeredCreditLine = MedalResult.RoundOfferedAmount(this.autoDecisionResponse.AutoApproveAmount);
            else if (this.autoDecisionResponse.IsAutoBankBasedApproval) {
                this.offeredCreditLine = MedalResult.RoundOfferedAmount(
                    this.autoDecisionResponse.BankBasedAutoApproveAmount
                );
            } else if (this.autoDecisionResponse.DecidedToReject)
                this.offeredCreditLine = 0;
        } // AdjustOfferredCreditLine

        private void CapOffer() {
            Log.Info("Finalizing and capping offer");

            this.offeredCreditLine = this.medal.RoundOfferedAmount();

            bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
                "GetIsCustomerHomeOwnerAccordingToLandRegistry",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerId", this.customerId)
            );

            if (isHomeOwnerAccordingToLandRegistry) {
                Log.Info("Capped for home owner according to land registry");
                this.offeredCreditLine = Math.Min(this.offeredCreditLine, MaxCapHomeOwner);
            } else {
                Log.Info("Capped for not home owner");
                this.offeredCreditLine = Math.Min(this.offeredCreditLine, MaxCapNotHomeOwner);
            } // if
        } // CapOffer

        private void FinishWizard() {
            if (this.finishWizardArgs == null)
                return;

            this.finishWizardArgs.DoMain = false;

            new FinishWizard(this.finishWizardArgs).Execute();
        } // FinishWizard

        private void GetLandRegistryData(List<CustomerAddressModel> addresses) {
            foreach (CustomerAddressModel address in addresses) {
                LandRegistryDataModel model = null;

                if (!string.IsNullOrEmpty(address.HouseName)) {
                    model = LandRegistryEnquiry.Get(this.customerId,
                        null,
                        address.HouseName,
                        null,
                        null,
                        address.PostCode
                    );
                } else if (!string.IsNullOrEmpty(address.HouseNumber)) {
                    model = LandRegistryEnquiry.Get(this.customerId,
                        address.HouseNumber,
                        null,
                        null,
                        null,
                        address.PostCode
                    );
                } else if (
                    !string.IsNullOrEmpty(address.FlatOrApartmentNumber) &&
                    string.IsNullOrEmpty(address.HouseNumber)
                ) {
                    model = LandRegistryEnquiry.Get(this.customerId,
                        address.FlatOrApartmentNumber,
                        null,
                        null,
                        null,
                        address.PostCode
                    );
                } // if

                bool doLandRegistry =
                    (model != null) &&
                    (model.Enquery != null) &&
                    (model.ResponseType == LandRegistryResponseType.Success) &&
                    (model.Enquery.Titles != null) &&
                    (model.Enquery.Titles.Count == 1);

                if (doLandRegistry) {
                    var lrr = new LandRegistryRes(this.customerId, model.Enquery.Titles[0].TitleNumber);
                    lrr.PartialExecute();

                    LandRegistry dbLandRegistry = lrr.LandRegistry;

                    LandRegistryDataModel landRegistryDataModel = lrr.RawResult;

                    if (landRegistryDataModel.ResponseType == LandRegistryResponseType.Success) {
                        // Verify customer is among owners
                        Customer customer = this.customers.Get(this.customerId);

                        bool isOwnerAccordingToLandRegistry = LandRegistryRes.IsOwner(
                            customer,
                            landRegistryDataModel.Response,
                            landRegistryDataModel.Res.TitleNumber
                        );

                        CustomerAddress dbAdress = this.customerAddressRepository.Get(address.AddressId);

                        dbLandRegistry.CustomerAddress = dbAdress;
                        this.landRegistryRepository.SaveOrUpdate(dbLandRegistry);

                        if (isOwnerAccordingToLandRegistry) {
                            dbAdress.IsOwnerAccordingToLandRegistry = true;
                            this.customerAddressRepository.SaveOrUpdate(dbAdress);
                        } // if
                    } // if
                } else {
                    int num = 0;

                    if (model != null && model.Enquery != null && model.Enquery.Titles != null)
                        num = model.Enquery.Titles.Count;

                    Log.Warn(
                        "No land registry retrieved for customer id: {5}," +
                        "house name: {0}, house number: {1}, flat number: {2}, postcode: {3}, num of enquries {4}",
                        address.HouseName,
                        address.HouseNumber,
                        address.FlatOrApartmentNumber,
                        address.PostCode,
                        num, this.customerId
                    );
                } // if
            } // for each
        } // GetLandRegistryData

        private void GetLandRegistryDataIfNotRejected() {
            var isHomeOwner = this.customerDetails.IsOwnerOfMainAddress || this.customerDetails.IsOwnerOfOtherProperties;

            if (!this.autoDecisionResponse.DecidedToReject && isHomeOwner) {
                Log.Debug(
                    "Retrieving LandRegistry system decision: {0} residential status: {1}",
                    this.autoDecisionResponse.DecisionName,
                    this.customerDetails.PropertyStatusDescription
                );

                var customerAddressesHelper = new CustomerAddressHelper(this.customerId);
                customerAddressesHelper.Execute();

                try {
                    GetLandRegistryData(customerAddressesHelper.OwnedAddresses);
                } catch (Exception e) {
                    Log.Error("Error while getting land registry data: {0}", e);
                } // try
            } else {
                Log.Info(
                    "Not retrieving LandRegistry system decision: {0} residential status: {1}",
                    this.autoDecisionResponse.DecisionName,
                    this.customerDetails.PropertyStatusDescription
                );
            } // if
        } // GetLandRegistryDataIfNotRejected

        private void ProcessApprovals() {
            bool bContinue = true;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (this.autoDecisionResponse.DecidedToReject && bContinue) {
                Log.Info("Not processing approvals: reject decision has been made.");
                bContinue = false;
            } // if

            if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision && bContinue) {
                Log.Info("Not processing approvals: {0} option selected.", this.newCreditLineOption);
                bContinue = false;
            } // if

            if (this.avoidAutomaticDecision == 1 && bContinue) {
                Log.Info("Not processing approvals: automatic decisions should be avoided.");
                bContinue = false;
            } // if

            if (!this.customerDetails.CustomerStatusIsEnabled && bContinue) {
                Log.Info("Not processing approvals: customer status is not enabled.");
                bContinue = false;
            } // if

            if (this.customerDetails.CustomerStatusIsWarning && bContinue) {
                Log.Info("Not processing approvals: customer status is 'warning'.");
                bContinue = false;
            } // if

            if (!EnableAutomaticReRejection && bContinue) {
                Log.Info("Not processing approvals: auto re-rejection is disabled.");
                bContinue = false;
            } // if

            if (!EnableAutomaticRejection && bContinue) {
                Log.Info("Not processing approvals: auto rejection is disabled.");
                bContinue = false;
            } // if

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (EnableAutomaticReApproval && bContinue) {
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                new AutoDecisionAutomation.AutoDecisions.ReApproval.Agent(this.customerId, DB, Log)
                    .Init()
                    .MakeDecision(this.autoDecisionResponse);

                bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

                if (!bContinue)
                    Log.Debug("Auto re-approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
            } else
                Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");

            if (EnableAutomaticApproval && bContinue) {
                new Approval(
                    this.customerId,
                    this.offeredCreditLine,
                    this.medal.MedalClassification,
                    (AutomationCalculator.Common.MedalType)this.medal.MedalType,
                    (AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
                    DB,
                    Log
                ).Init().MakeDecision(this.autoDecisionResponse);

                bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

                if (!bContinue)
                    Log.Debug("Auto approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
            } else {
                Log.Debug(
                    "Not processed auto approval: " +
                    "it is currently disabled in configuration or decision has already been made earlier."
                );
            } // if

            if (CurrentValues.Instance.BankBasedApprovalIsEnabled && bContinue) {
                new BankBasedApproval(this.customerId).MakeDecision(this.autoDecisionResponse);

                bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

                if (!bContinue)
                    Log.Debug("Bank based approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
            } else {
                Log.Debug(
                    "Not processed bank based approval: " +
                    "it is currently disabled in configuration or decision has already been made earlier."
                );
            } // if

            if (!this.autoDecisionResponse.SystemDecision.HasValue) { // No decision is made so far
                this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
                this.autoDecisionResponse.UserStatus = Status.Manual;
                this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;

                Log.Debug("Not approval has reached decision: setting it to be 'waiting for decision'.");
            } // if
        } // ProcessApprovals

        private void ProcessRejections() {
            if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision)
                return;

            if (this.avoidAutomaticDecision == 1)
                return;

            if (EnableAutomaticReRejection)
                new ReRejection(this.customerId, DB, Log).MakeDecision(this.autoDecisionResponse);

            if (this.autoDecisionResponse.IsReRejected)
                return;

            if (!EnableAutomaticRejection)
                return;

            if (this.customerDetails.IsAlibaba)
                return;

            new Agent(this.customerId, DB, Log).Init().MakeDecision(this.autoDecisionResponse);
        } // ProcessRejections

        /// <summary>
        /// Last stage of auto-decision process
        /// </summary>
        private void UpdateCustomerAndCashRequest() {
            var now = DateTime.UtcNow;

            decimal interestRateToUse;
            decimal setupFeePercentToUse;
            int repaymentPeriodToUse;
            LoanType loanTypeIdToUse;

            if (this.autoDecisionResponse.IsAutoApproval) {
                interestRateToUse = this.autoDecisionResponse.InterestRate;
                setupFeePercentToUse = this.autoDecisionResponse.SetupFee;
                repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
                loanTypeIdToUse =
                    this.loanTypeRepository.Get(this.autoDecisionResponse.LoanTypeID) ??
                    this.loanTypeRepository.GetDefault();
            } else {
                //TODO check this code!!!
                interestRateToUse = this.lastOffer.LoanOfferInterestRate;
                setupFeePercentToUse = this.lastOffer.ManualSetupFeePercent;
                repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
                loanTypeIdToUse = this.loanTypeRepository.GetDefault();
            } // if

            var customer = this.customers.Get(this.customerId);

            if (customer == null)
                return;

            if (this.overrideApprovedRejected) {
                customer.CreditResult = this.autoDecisionResponse.CreditResult;
                customer.Status = this.autoDecisionResponse.UserStatus;
            } // if

            customer.OfferStart = now;
            customer.OfferValidUntil = now.AddHours(CurrentValues.Instance.OfferValidForHours);
            customer.SystemDecision = this.autoDecisionResponse.SystemDecision;
            customer.Medal = this.medal.MedalClassification;
            customer.CreditSum = this.offeredCreditLine;
            customer.LastStatus = this.autoDecisionResponse.CreditResult.HasValue
                ? this.autoDecisionResponse.CreditResult.ToString()
                : "N/A";
            customer.SystemCalculatedSum = this.medal.RoundOfferedAmount();
            customer.ManagerApprovedSum = this.offeredCreditLine;
            if (this.autoDecisionResponse.DecidedToReject) {
                customer.DateRejected = now;
                customer.RejectedReason = this.autoDecisionResponse.DecisionName;
                customer.NumRejects++;
            } // if

            if (this.autoDecisionResponse.DecidedToApprove) {
                customer.DateApproved = now;
                customer.ApprovedReason = this.autoDecisionResponse.DecisionName;
                customer.NumApproves++;
                customer.IsLoanTypeSelectionAllowed = 1;
            } // if

            var cr = customer.LastCashRequest;

            if (cr != null) {
                cr.OfferStart = customer.OfferStart;
                cr.OfferValidUntil = customer.OfferValidUntil;

                cr.SystemDecision = this.autoDecisionResponse.SystemDecision;
                cr.SystemCalculatedSum = this.medal.RoundOfferedAmount();
                cr.SystemDecisionDate = now;
                cr.ManagerApprovedSum = this.offeredCreditLine;
                cr.UnderwriterDecision = this.autoDecisionResponse.CreditResult;
                cr.UnderwriterDecisionDate = now;
                cr.UnderwriterComment = this.autoDecisionResponse.DecisionName;
                cr.AutoDecisionID = this.autoDecisionResponse.DecisionCode;
                cr.MedalType = this.medal.MedalClassification;
                cr.ScorePoints = (double)this.medal.TotalScoreNormalized;
                cr.ExpirianRating = this.customerDetails.ExperianConsumerScore;
                cr.AnnualTurnover = (int)this.medal.AnnualTurnover;
                cr.LoanType = loanTypeIdToUse;
                cr.LoanSource = this.loanSourceRepository.GetDefault();

                if (this.autoDecisionResponse.DecidedToApprove)
                    cr.InterestRate = interestRateToUse;

                if (repaymentPeriodToUse != 0) {
                    cr.ApprovedRepaymentPeriod = repaymentPeriodToUse;
                    cr.RepaymentPeriod = repaymentPeriodToUse;
                } // if

                cr.ManualSetupFeePercent = setupFeePercentToUse;
                cr.APR = this.lastOffer.LoanOfferApr;

                if (this.autoDecisionResponse.IsAutoReApproval) {
                    cr.EmailSendingBanned = this.autoDecisionResponse.LoanOfferEmailSendingBannedNew;
                    cr.IsCustomerRepaymentPeriodSelectionAllowed =
                        this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed;
                    cr.DiscountPlan = this.autoDecisionResponse.DiscountPlanID.HasValue
                        ? this.discountPlanRepository.Get(this.autoDecisionResponse.DiscountPlanID.Value)
                        : this.discountPlanRepository.GetDefault();

                    cr.LoanSource = this.loanSourceRepository.Get(this.autoDecisionResponse.LoanSourceID);

                    if (cr.LoanSource.MaxInterest.HasValue && cr.InterestRate > cr.LoanSource.MaxInterest.Value) {
                        Log.Warn("too big interest was assigned for this loan source - adjusting for customer {0}", this.customerId);
                        cr.InterestRate = cr.LoanSource.MaxInterest.Value;
                    }

                    if (cr.LoanSource.DefaultRepaymentPeriod.HasValue && cr.ApprovedRepaymentPeriod < cr.LoanSource.DefaultRepaymentPeriod) {
                        Log.Warn("too small repayment period was assigned for this loan source - adjusting for customer {0}", this.customerId);
                        cr.ApprovedRepaymentPeriod = cr.LoanSource.DefaultRepaymentPeriod;
                        cr.RepaymentPeriod = cr.LoanSource.DefaultRepaymentPeriod.Value;
                    }

                    if (cr.LoanSource.IsCustomerRepaymentPeriodSelectionAllowed == false && cr.IsCustomerRepaymentPeriodSelectionAllowed == true) {
                        Log.Warn("wrong customer repayment period was assigned for this loan source - adjusting for customer {0}", this.customerId);
                        cr.IsCustomerRepaymentPeriodSelectionAllowed = false;
                    }

                    cr.LoanType = this.loanTypeRepository.Get(this.autoDecisionResponse.LoanTypeID);
                    cr.ManualSetupFeePercent = this.autoDecisionResponse.SetupFee;
                    
                    AddNewDecisionOffer(now, cr);
                } // if
            } // if

            customer.LastStartedMainStrategyEndTime = now;

            this.customers.SaveOrUpdate(customer);


            //TODO update new offer / decision tables
            Log.Info("update new offer / decision for customer {0}", customer.Id);

            if (this.autoDecisionResponse.Decision.HasValue) {
                this.decisionHistory.LogAction(
                    this.autoDecisionResponse.Decision.Value,
                    this.autoDecisionResponse.DecisionName,
                    this.session.Get<User>(1), customer
                );
            } // if

            // TEMPORARY DISABLED TODO - sync for proper launch
            UpdateSalesForceOpportunity(customer.Name);

            if (customer.IsAlibaba)
                UpdatePartnerAlibaba(customer.Id);

        }// UpdateCustomerAndCashRequest

        private void AddNewDecisionOffer(DateTime now, CashRequest cr) {
            AddDecision addDecisionStra = new AddDecision(new NL_Decisions {
                DecisionNameID = this.autoDecisionResponse.Decision.HasValue ? (int)this.autoDecisionResponse.Decision.Value : (int)DecisionActions.Waiting,
                DecisionTime = now,
                InterestOnlyRepaymentCount = 0, //todo
                IsAmountSelectionAllowed = cr.IsLoanTypeSelectionAllowed == 1, //todo
                IsRepaymentPeriodSelectionAllowed = cr.IsCustomerRepaymentPeriodSelectionAllowed,
                Notes = this.autoDecisionResponse.CreditResult.HasValue ? this.autoDecisionResponse.CreditResult.Value.DescriptionAttr() : "",
                // todo Position = 
                // todo CashRequestID = 
                SendEmailNotification = !cr.EmailSendingBanned,
                UserID = 1,
            }, cr.Id);

            addDecisionStra.Execute();
            int decisionID = addDecisionStra.DecisionID;

            AddOffer addOfferStra = new AddOffer(new NL_Offers {
                DecisionID = decisionID,
                Amount = this.offeredCreditLine,
                // todo BrokerSetupFeePercent = 0 
                CreatedTime = now,
                DiscountPlanID = cr.DiscountPlan.Id,
                EmailSendingBanned = cr.EmailSendingBanned,
                InterestOnlyRepaymentCount = 0, //todo
                IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed == 1,
                LoanSourceID = this.autoDecisionResponse.LoanSourceID,
                LoanTypeID = this.autoDecisionResponse.LoanTypeID,
                MonthlyInterestRate = this.autoDecisionResponse.InterestRate,
                RepaymentCount = this.autoDecisionResponse.RepaymentPeriod,
                RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month, //todo
                SetupFeePercent = this.autoDecisionResponse.SetupFee,
                StartTime = now,
                EndTime = now.AddHours(CurrentValues.Instance.OfferValidForHours)
            });
            addOfferStra.Execute();
            int offerID = addOfferStra.OfferID;
        }//AddNewDecisionOffer



        private void UpdateSalesForceOpportunity(string customerEmail) {
            new AddUpdateLeadAccount(customerEmail, this.customerId, false, false).Execute();

            if (!this.autoDecisionResponse.Decision.HasValue)
                return;

            switch (this.autoDecisionResponse.Decision.Value) {
                case DecisionActions.Approve:
                case DecisionActions.ReApprove:
                    new UpdateOpportunity(this.customerId, new OpportunityModel {
                        ApprovedAmount = this.autoDecisionResponse.AutoApproveAmount,
                        Email = customerEmail,
                        ExpectedEndDate = this.autoDecisionResponse.AppValidFor,
                        Stage = OpportunityStage.s90.DescriptionAttr(),
                    }).Execute();
                    break;

                case DecisionActions.Reject:
                case DecisionActions.ReReject:
                    new UpdateOpportunity(this.customerId, new OpportunityModel {
                        Email = customerEmail,
                        DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
                        DealLostReason = "Auto " + this.autoDecisionResponse.Decision.Value.ToString(),
                        CloseDate = DateTime.UtcNow
                    }).Execute();
                    break;
            } // switch
        } // UpdateSalesForceOpportunity

        private void ExecuteAdditionalStrategies() {
            var preData = new PreliminaryData(this.customerId);

            var strat = new ExperianConsumerCheck(this.customerId, null, false);
            strat.Execute();

            if (preData.TypeOfBusiness != "Entrepreneur") {
                Library.Instance.DB.ForEachRowSafe(
                    sr => {
                        int appDirId = sr["DirId"];
                        string appDirName = sr["DirName"];
                        string appDirSurname = sr["DirSurname"];

                        if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
                            return;

                        var directorExperianConsumerCheck = new ExperianConsumerCheck(this.customerId, appDirId, false);
                        directorExperianConsumerCheck.Execute();
                    },
                    "GetCustomerDirectorsForConsumerCheck",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("CustomerId", this.customerId)
                );
            } // if

            if (preData.LastStartedMainStrategyEndTime.HasValue) {
                Library.Instance.Log.Info("Performing experian company check");
                var experianCompanyChecker = new ExperianCompanyCheck(this.customerId, false);
                experianCompanyChecker.Execute();
            } // if

            if (preData.LastStartedMainStrategyEndTime.HasValue)
                new AmlChecker(this.customerId).Execute();

            bool shouldRunBwa =
                preData.AppBankAccountType == "Personal" &&
                preData.BwaBusinessCheck == "1" &&
                preData.AppSortCode != null &&
                preData.AppAccountNumber != null;

            if (shouldRunBwa)
                new BwaChecker(this.customerId).Execute();

            Library.Instance.Log.Info("Getting Zoopla data for customer {0}", this.customerId);
            new ZooplaStub(this.customerId).Execute();
        } // ExecuteAdditionalStrategies

        private bool EnableAutomaticApproval { get { return CurrentValues.Instance.EnableAutomaticApproval; } }
        private bool EnableAutomaticReApproval { get { return CurrentValues.Instance.EnableAutomaticReApproval; } }
        private bool EnableAutomaticRejection { get { return CurrentValues.Instance.EnableAutomaticRejection; } }
        private bool EnableAutomaticReRejection { get { return CurrentValues.Instance.EnableAutomaticReRejection; } }
        private int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
        private int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

        private readonly CustomerRepository customers;
        private readonly DecisionHistoryRepository decisionHistory;
        private readonly DiscountPlanRepository discountPlanRepository;
        private readonly LoanSourceRepository loanSourceRepository;
        private readonly LoanTypeRepository loanTypeRepository;
        private readonly ISession session;
        private readonly int avoidAutomaticDecision;
        private readonly CustomerAddressRepository customerAddressRepository;
        private readonly LandRegistryRepository landRegistryRepository;

        // Inputs
        private readonly int customerId;
        private readonly FinishWizardArgs finishWizardArgs;

        // Helpers
        private readonly NewCreditLineOption newCreditLineOption;
        private readonly AutoDecisionResponse autoDecisionResponse;

        private CustomerDetails customerDetails;
        private LastOfferData lastOffer;

        private MedalResult medal;

        private int offeredCreditLine;

        /// <summary>
        /// Default: true. However when Main strategy is executed as a part of
        /// Finish Wizard strategy and customer is already approved/rejected
        /// then customer's status should not change.
        /// </summary>
        private bool overrideApprovedRejected;

        /// <summary>
        /// Auto decision only treated 
        /// In case of auto decision occurred (RR, R, RA, A), 002 sent immediately
        /// In the case of "Waiting"/manual, 002 will be transmitted in UI underwrites,
        /// CustomerController, ChangeStatus method.
        /// </summary>
        /// <param name="customerID"></param>
        private void UpdatePartnerAlibaba(int customerID) {
            DecisionActions autoDecision = this.autoDecisionResponse.Decision ?? DecisionActions.Waiting;

            Log.Info(
                "UpdatePartnerAlibaba ******************************************************{0}, {1}",
                customerID,
                autoDecision
            );

            //	Reject, Re-Reject, Re-Approve, Approve: 0001 + 0002 (auto decision is a final also)
            // other: 0001 
            switch (autoDecision) {
                case DecisionActions.ReReject:
                case DecisionActions.Reject:
                case DecisionActions.ReApprove:
                case DecisionActions.Approve:
                    new DataSharing(customerID, AlibabaBusinessType.APPLICATION).Execute();
                    new DataSharing(customerID, AlibabaBusinessType.APPLICATION_REVIEW).Execute();
                    break;

                // auto not final
                case DecisionActions.Waiting:
                    new DataSharing(customerID, AlibabaBusinessType.APPLICATION).Execute();
                    break;

                default:  // unknown auto decision status
                    throw new StrategyAlert(
                        this,
                        string.Format("Auto decision invalid value {0} for customer {1}", autoDecision, customerID)
                    );
            } // switch
        } // UpdatePartnerAlibaba

        private void ValidateCashRequestArgs() {
            if (this.finishWizardArgs != null) {
                SafeReader sr = DB.GetFirst(
                    "GetCashRequestData",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("@CustomerId", this.customerId)
                );

                this.cashRequestID = sr["Id"];
                this.createCashRequest = DoAction.No;
                this.updateCashRequest = DoAction.Yes;
            } // if

            bool crIsNotNull = (this.cashRequestID != null);
            bool doCreate = (this.createCashRequest == DoAction.Yes);

            if (crIsNotNull == doCreate) {
                throw new StrategyException(
                    this,
                    "Cannot execute Main strategy: cash request " +
                    (crIsNotNull ? "not specified, Create not" : "specified, Create") +
                    " requested."
                );
            } // if
        } // ValidateCashRequestArgs
    } // class MainStrategy
} // namespace
