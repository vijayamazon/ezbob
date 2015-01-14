namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Backend.Strategies.ScoreCalculation;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using LandRegistryLib;
	using NHibernate;
	using SalesForceLib.Models;
	using StructureMap;

	public partial class MainStrategy : AStrategy {
		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			FinishWizardArgs fwa
		) {
			this.finishWizardArgs = fwa;

			this._session = ObjectFactory.GetInstance<ISession>();
			this._customers = ObjectFactory.GetInstance<CustomerRepository>();
			this._decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
			this._loanSourceRepository = ObjectFactory.GetInstance<LoanSourceRepository>();
			this._loanTypeRepository = ObjectFactory.GetInstance<LoanTypeRepository>();
			this._discountPlanRepository = ObjectFactory.GetInstance<DiscountPlanRepository>();
			this.customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
			this.landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();

			this.medalScoreCalculator = new MedalScoreCalculator(DB, Log);

			this.mailer = new StrategiesMailer();
			this.customerId = customerId;
			this.newCreditLineOption = newCreditLine;
			this.avoidAutomaticDecision = avoidAutoDecision;
			this.overrideApprovedRejected = true;
			this.staller = new Staller(customerId, this.newCreditLineOption, this.mailer, DB, Log);
			this.dataGatherer = new DataGatherer(customerId, DB, Log);
		} // constructor

		public override void Execute() {
			if (this.finishWizardArgs != null)
				FinishWizard();

			this.autoDecisionResponse = new AutoDecisionResponse {
				DecisionName = "Manual",
			};

			if (this.newCreditLineOption == NewCreditLineOption.SkipEverything) {
				Log.Alert("MainStrategy was activated in SkipEverything mode. Nothing is done. Avoid such calls!");
				return;
			} // if

			// Wait for data to be filled by other strategies
			if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules)
				this.staller.Stall();

			this.dataGatherer.GatherPreliminaryData();
			this.wasMainStrategyExecutedBefore = this.dataGatherer.LastStartedMainStrategyEndTime.HasValue;

			if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules) {
				ExecuteAdditionalStrategies(
					this.customerId,
					this.wasMainStrategyExecutedBefore,
					this.dataGatherer.TypeOfBusiness,
					this.dataGatherer.BwaBusinessCheck,
					this.dataGatherer.AppBankAccountType,
					this.dataGatherer.AppAccountNumber,
					this.dataGatherer.AppSortCode
				);
			} // if

			this.dataGatherer.Gather();

			if (!this.dataGatherer.IsTest) {
				var fraudChecker = new FraudChecker(this.customerId, FraudMode.FullCheck);
				fraudChecker.Execute();
			} // if

			// Processing logic
			this.isHomeOwner = this.dataGatherer.IsOwnerOfMainAddress || this.dataGatherer.IsOwnerOfOtherProperties;
			this.isFirstLoan = this.dataGatherer.NumOfLoans == 0;

			// Calculate old medal
			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();
			this.modelLoanOffer = scoringResult.MaxOffer;

			ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			bool bSkip = 
				this.newCreditLineOption == NewCreditLineOption.SkipEverything ||
				this.newCreditLineOption == NewCreditLineOption.SkipEverythingAndApplyAutoRules;

			if (!bSkip)
				GetLandRegistryDataIfNotRejected();

			CalculateNewMedal();

			CapOffer();

			ProcessApprovals();

			AdjustOfferredCreditLine();

			UpdateCustomerAndCashRequest();

			SendEmails();
		} // Execute

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected) {
			this.overrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		} // SetOverrideApprovedRejected

		private void AdjustOfferredCreditLine() {
			if (this.autoDecisionResponse.IsAutoReApproval || this.autoDecisionResponse.IsAutoApproval)
				this.offeredCreditLine = RoundOfferedAmount(this.autoDecisionResponse.AutoApproveAmount);
			else if (this.autoDecisionResponse.IsAutoBankBasedApproval)
				this.offeredCreditLine = RoundOfferedAmount(this.autoDecisionResponse.BankBasedAutoApproveAmount);
			else if (this.autoDecisionResponse.DecidedToReject)
				this.offeredCreditLine = 0;
		} // AdjustOfferredCreditLine

		private void CalculateNewMedal() {
			var instance = new CalculateMedal(
				this.customerId,
				this.dataGatherer.TypeOfBusiness,
				this.dataGatherer.MaxExperianConsumerScore,
				this.dataGatherer.MaxCompanyScore,
				this.dataGatherer.NumOfHmrcMps,
				this.dataGatherer.NumOfYodleeMps,
				this.dataGatherer.NumOfEbayAmazonPayPalMps,
				this.dataGatherer.EarliestHmrcLastUpdateDate,
				this.dataGatherer.EarliestYodleeLastUpdateDate
			);
			instance.Execute();

			this.medalClassification = instance.Result.MedalClassification;
			this.medalScore = instance.Result.TotalScoreNormalized;
			this.medalType = instance.Result.MedalType;
			this.turnoverType = instance.Result.TurnoverType;

			this.modelLoanOffer = RoundOfferedAmount(instance.Result.OfferedLoanAmount);
		} // CalculateNewMedal

		private ScoreMedalOffer CalculateScoreAndMedal() {
			Log.Info("Calculating score & medal");

			ScoreMedalOffer scoringResult = this.medalScoreCalculator.CalculateMedalScore(
				this.dataGatherer.TotalSumOfOrdersForLoanOffer,
				this.dataGatherer.MinExperianConsumerScore,
				this.dataGatherer.MarketplaceSeniorityYears,
				this.dataGatherer.ModelMaxFeedback,
				this.dataGatherer.MaritalStatus,
				this.dataGatherer.AppGender == "M" ? Gender.M : Gender.F,
				this.dataGatherer.NumOfEbayAmazonPayPalMps,
				this.dataGatherer.FirstRepaymentDatePassed,
				this.dataGatherer.EzbobSeniorityMonths,
				this.dataGatherer.ModelOnTimeLoans,
				this.dataGatherer.ModelLatePayments,
				this.dataGatherer.ModelEarlyPayments
			);

			// Save online medal
			DB.ExecuteNonQuery(
				"CustomerScoringResult_Insert",
				CommandSpecies.StoredProcedure,
				new QueryParameter("pCustomerId", this.customerId),
				new QueryParameter("pAC_Parameters", scoringResult.AcParameters),
				new QueryParameter("AC_Descriptors", scoringResult.AcDescriptors),
				new QueryParameter("Result_Weight", scoringResult.ResultWeigts),
				new QueryParameter("pResult_MAXPossiblePoints", scoringResult.ResultMaxPoints),
				new QueryParameter("pMedal", scoringResult.Medal.ToString()),
				new QueryParameter("pScorePoints", scoringResult.ScorePoints),
				new QueryParameter("pScoreResult", scoringResult.ScoreResult)
			);

			// Update CustomerAnalyticsLocalData
			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateLocalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerId),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow),
				new QueryParameter("AnnualTurnover", this.dataGatherer.TotalSumOfOrders1YTotal),
				new QueryParameter("TotalSumOfOrdersForLoanOffer", this.dataGatherer.TotalSumOfOrdersForLoanOffer),
				new QueryParameter("MarketplaceSeniorityYears", this.dataGatherer.MarketplaceSeniorityYears),
				new QueryParameter("MaxFeedback", this.dataGatherer.ModelMaxFeedback),
				new QueryParameter("MPsNumber", this.dataGatherer.NumOfEbayAmazonPayPalMps),
				new QueryParameter("FirstRepaymentDatePassed", this.dataGatherer.FirstRepaymentDatePassed),
				new QueryParameter("EzbobSeniorityMonths", this.dataGatherer.EzbobSeniorityMonths),
				new QueryParameter("OnTimeLoans", this.dataGatherer.ModelOnTimeLoans),
				new QueryParameter("LatePayments", this.dataGatherer.ModelLatePayments),
				new QueryParameter("EarlyPayments", this.dataGatherer.ModelEarlyPayments)
			);

			return scoringResult;
		} // CalculateScoreAndMedal

		private void CapOffer() {
			Log.Info("Finalizing and capping offer");

			this.offeredCreditLine = this.modelLoanOffer;

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				this.offeredCreditLine = Math.Min(this.offeredCreditLine, this.dataGatherer.MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				this.offeredCreditLine = Math.Min(this.offeredCreditLine, this.dataGatherer.MaxCapNotHomeOwner);
			} // if
		} // CapOffer

		private void FinishWizard() {
			if (this.finishWizardArgs == null)
				return;

			this.finishWizardArgs.DoMain = false;

			new FinishWizard(this.finishWizardArgs).Execute();
		} // FinishWizard

		private void GetLandRegistry() {
			var customerAddressesHelper = new CustomerAddressHelper(this.customerId);
			customerAddressesHelper.Execute();

			try {
				GetLandRegistryData(customerAddressesHelper.OwnedAddresses);
			} catch (Exception e) {
				Log.Error("Error while getting land registry data: {0}", e);
			} // try
		} // GetLandRegistry

		private void GetLandRegistryData(List<CustomerAddressModel> addresses) {
			foreach (CustomerAddressModel address in addresses) {
				LandRegistryDataModel model = null;

				if (!string.IsNullOrEmpty(address.HouseName)) {
					model = LandRegistryEnquiry.Get(
						customerId,
						null,
						address.HouseName,
						null,
						null,
						address.PostCode
					);
				} else if (!string.IsNullOrEmpty(address.HouseNumber)) {
					model = LandRegistryEnquiry.Get(
						customerId,
						address.HouseNumber,
						null,
						null,
						null,
						address.PostCode
					);
				} else if (!string.IsNullOrEmpty(address.FlatOrApartmentNumber) && string.IsNullOrEmpty(address.HouseNumber)) {
					model = LandRegistryEnquiry.Get(
						customerId,
						address.FlatOrApartmentNumber,
						null,
						null,
						null,
						address.PostCode
					);
				}

				if (model != null && model.Enquery != null && model.ResponseType == LandRegistryResponseType.Success && model.Enquery.Titles != null &&
					model.Enquery.Titles.Count == 1) {

					var lrr = new LandRegistryRes(customerId, model.Enquery.Titles[0].TitleNumber);
					lrr.PartialExecute();
					LandRegistry dbLandRegistry = lrr.LandRegistry;
					LandRegistryDataModel landRegistryDataModel = lrr.RawResult;

					if (landRegistryDataModel.ResponseType == LandRegistryResponseType.Success) {
						// Verify customer is among owners
						Customer customer = _customers.Get(customerId);
						bool isOwnerAccordingToLandRegistry = LandRegistryRes.IsOwner(customer, landRegistryDataModel.Response, landRegistryDataModel.Res.TitleNumber);
						CustomerAddress dbAdress = customerAddressRepository.Get(address.AddressId);

						dbLandRegistry.CustomerAddress = dbAdress;
						landRegistryRepository.SaveOrUpdate(dbLandRegistry);

						if (isOwnerAccordingToLandRegistry) {
							dbAdress.IsOwnerAccordingToLandRegistry = true;
							customerAddressRepository.SaveOrUpdate(dbAdress);
						}
					}
				} else {
					int num = 0;
					if (model != null && model.Enquery != null && model.Enquery.Titles != null)
						num = model.Enquery.Titles.Count;
					Log.Warn(
						"No land registry retrieved for customer id: {5}, house name: {0}, house number: {1}, flat number: {2}, postcode: {3}, num of enquries {4}",
						address.HouseName, address.HouseNumber,
						address.FlatOrApartmentNumber, address.PostCode, num, customerId);
				}
			}
		}

		private void GetLandRegistryDataIfNotRejected() {
			if (!this.autoDecisionResponse.DecidedToReject && this.isHomeOwner) {
				Log.Debug(
					"Retrieving LandRegistry system decision: {0} residential status: {1}",
					this.autoDecisionResponse.DecisionName,
					this.dataGatherer.PropertyStatusDescription
					);
				GetLandRegistry();
			} else {
				Log.Info(
					"Not retrieving LandRegistry system decision: {0} residential status: {1}",
					this.autoDecisionResponse.DecisionName,
					this.dataGatherer.PropertyStatusDescription
				);
			} // if
		} // GetLandRegistryDataIfNotRejected

		private void ProcessApprovals() {
			bool bContinue = true; 
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

			if (!this.dataGatherer.CustomerStatusIsEnabled && bContinue) {
				Log.Info("Not processing approvals: customer status is not enabled.");
				bContinue = false;
			} // if

			if (this.dataGatherer.CustomerStatusIsWarning && bContinue) {
				Log.Info("Not processing approvals: customer status is 'warning'.");
				bContinue = false;
			} // if

			if ((!this.dataGatherer.EnableAutomaticReRejection || !this.dataGatherer.EnableAutomaticRejection) && bContinue) {
				Log.Info("Not processing approvals: auto rejection or auto re-rejection is disabled.");
				bContinue = false;
			} // if

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if (this.dataGatherer.EnableAutomaticReApproval && bContinue) {
				// ReSharper restore ConditionIsAlwaysTrueOrFalse
				new AutoDecisions.ReApproval.Agent(this.customerId, DB, Log).Init().MakeDecision(this.autoDecisionResponse);

				bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Auto re-approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
			} else
				Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");

			if (this.dataGatherer.EnableAutomaticApproval && bContinue) {
				new Approval(this.customerId, this.offeredCreditLine, this.medalClassification,
					(AutomationCalculator.Common.MedalType)this.medalType,
					(AutomationCalculator.Common.TurnoverType?)this.turnoverType,
					DB,
					Log
					).Init()
					.MakeDecision(this.autoDecisionResponse);

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

			if (this.dataGatherer.EnableAutomaticReRejection)
				new ReRejection(this.customerId, DB, Log).MakeDecision(this.autoDecisionResponse);

			if (this.autoDecisionResponse.IsReRejected)
				return;

			if (!this.dataGatherer.EnableAutomaticRejection)
				return;

			if (this.dataGatherer.IsAlibaba)
				return;

			new AutoDecisions.Reject.Agent(this.customerId, DB, Log).Init().MakeDecision(this.autoDecisionResponse);
		} // ProcessRejections

		private int RoundOfferedAmount(decimal amount) {
			return (int)Math.Truncate(amount / CurrentValues.Instance.GetCashSliderStep) *
				CurrentValues.Instance.GetCashSliderStep;
		} // RoundOfferedAmount
		
		private void UpdateCustomerAndCashRequest() {
			var now = DateTime.UtcNow;

			decimal interestRateToUse;
			decimal setupFeePercentToUse, setupFeeAmountToUse;
			int repaymentPeriodToUse;
			LoanType loanTypeIdToUse;
			bool isEuToUse = false;

			if (this.autoDecisionResponse.IsAutoApproval) {
				interestRateToUse = this.autoDecisionResponse.InterestRate;
				setupFeePercentToUse = this.autoDecisionResponse.SetupFee;
				setupFeeAmountToUse = setupFeePercentToUse * this.offeredCreditLine;
				repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
				isEuToUse = false; //todo implement when there are requirement
				loanTypeIdToUse =
					this._loanTypeRepository.Get(this.autoDecisionResponse.LoanTypeID) ??
					this._loanTypeRepository.GetDefault();
			} else {

				//TODO check this code!!!
				interestRateToUse = this.dataGatherer.LoanOfferInterestRate;
				setupFeePercentToUse = this.dataGatherer.ManualSetupFeePercent;
				setupFeeAmountToUse = this.dataGatherer.ManualSetupFeeAmount;
				repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
				loanTypeIdToUse = this._loanTypeRepository.GetDefault();
			} // if

			var customer = this._customers.Get(this.customerId);

			if (customer == null)
				return;

			if (this.overrideApprovedRejected) {
				customer.CreditResult = this.autoDecisionResponse.CreditResult;
				customer.Status = this.autoDecisionResponse.UserStatus;
			} // if

			customer.OfferStart = now;
			customer.OfferValidUntil = now.AddHours(CurrentValues.Instance.OfferValidForHours);
			customer.SystemDecision = this.autoDecisionResponse.SystemDecision;
			customer.Medal = this.medalClassification;
			customer.CreditSum = this.offeredCreditLine;
			customer.LastStatus = this.autoDecisionResponse.CreditResult.HasValue
				? this.autoDecisionResponse.CreditResult.ToString()
				: "N/A";
			customer.SystemCalculatedSum = this.modelLoanOffer;

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
				cr.SystemCalculatedSum = this.modelLoanOffer;
				cr.SystemDecisionDate = now;
				cr.ManagerApprovedSum = this.offeredCreditLine;
				cr.UnderwriterDecision = this.autoDecisionResponse.CreditResult;
				cr.UnderwriterDecisionDate = now;
				cr.UnderwriterComment = this.autoDecisionResponse.DecisionName;
				cr.AutoDecisionID = this.autoDecisionResponse.DecisionCode;
				cr.MedalType = this.medalClassification;
				cr.ScorePoints = (double)this.medalScore;
				cr.ExpirianRating = this.dataGatherer.ExperianConsumerScore;
				cr.AnnualTurnover = (int)this.dataGatherer.TotalSumOfOrders1YTotal;
				cr.LoanType = loanTypeIdToUse;
				cr.LoanSource = isEuToUse
					? this._loanSourceRepository.GetByName(LoanSourceName.EU.ToString())
					: this._loanSourceRepository.GetDefault();

				if (this.autoDecisionResponse.DecidedToApprove)
					cr.InterestRate = interestRateToUse;

				if (repaymentPeriodToUse != 0) {
					cr.ApprovedRepaymentPeriod = repaymentPeriodToUse;
					cr.RepaymentPeriod = repaymentPeriodToUse;
				} // if

				cr.ManualSetupFeeAmount = (int)setupFeeAmountToUse;
				cr.ManualSetupFeePercent = setupFeePercentToUse;
				cr.UseSetupFee = setupFeeAmountToUse > 0 || setupFeePercentToUse > 0;
				cr.APR = this.dataGatherer.LoanOfferApr;

				if (autoDecisionResponse.IsAutoReApproval) {
					cr.UseSetupFee = autoDecisionResponse.SetupFeeEnabled;
					cr.EmailSendingBanned = autoDecisionResponse.LoanOfferEmailSendingBannedNew;
					cr.IsCustomerRepaymentPeriodSelectionAllowed = autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed;
					cr.DiscountPlan = autoDecisionResponse.DiscountPlanID.HasValue ? _discountPlanRepository.Get(autoDecisionResponse.DiscountPlanID.Value) : null;
					cr.UseBrokerSetupFee = autoDecisionResponse.BrokerSetupFeeEnabled;
					cr.LoanSource = _loanSourceRepository.Get(autoDecisionResponse.LoanSourceID);
					cr.LoanType = _loanTypeRepository.Get(autoDecisionResponse.LoanTypeID);
					cr.ManualSetupFeeAmount = autoDecisionResponse.ManualSetupFeeAmount;
					cr.ManualSetupFeePercent = autoDecisionResponse.ManualSetupFeePercent;
				} // if
			} // if

			customer.LastStartedMainStrategyEndTime = now;

			this._customers.SaveOrUpdate(customer);

			if (this.autoDecisionResponse.Decision.HasValue) {
				this._decisionHistory.LogAction(
					this.autoDecisionResponse.Decision.Value,
					this.autoDecisionResponse.DecisionName,
					this._session.Get<User>(1), customer
				);
			} // if

			UpdateSalesForceOpportunity(customer.Name);
		}// UpdateCustomerAndCashRequest

		private void UpdateSalesForceOpportunity(string customerEmail) {
			new AddUpdateLeadAccount(customerEmail, customerId, false, false).Execute();

			if (!autoDecisionResponse.Decision.HasValue) {
				return;
			}

			switch (autoDecisionResponse.Decision.Value) {
			case DecisionActions.Approve:
			case DecisionActions.ReApprove:
				new UpdateOpportunity(customerId, new OpportunityModel {
					ApprovedAmount = autoDecisionResponse.AutoApproveAmount,
					Email = customerEmail,
					ExpectedEndDate = autoDecisionResponse.AppValidFor,
					Stage = (int)OpportunityStage.s90 //todo
				}).Execute();
				break;
			case DecisionActions.Reject:
			case DecisionActions.ReReject:
				new UpdateOpportunity(customerId, new OpportunityModel {
					Email = customerEmail,
					DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
					DealLostReason = "Auto " + autoDecisionResponse.Decision.Value.ToString(),
					CloseDate = DateTime.UtcNow
				}).Execute();
				break;
			}
		}

		private static void ExecuteAdditionalStrategies(
			int customerId,
			bool wasMainStrategyExecutedBefore,
			string typeOfBusiness,
			string bwaBusinessCheck,
			string appBankAccountType,
			string appAccountNumber,
			string appSortCode
		) {
			var strat = new ExperianConsumerCheck(customerId, null, false);
			strat.Execute();

			if (typeOfBusiness != "Entrepreneur") {
				Library.Instance.DB.ForEachRowSafe(
					sr => {
						int appDirId = sr["DirId"];
						string appDirName = sr["DirName"];
						string appDirSurname = sr["DirSurname"];

						if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
							return;

						var directorExperianConsumerCheck = new ExperianConsumerCheck(customerId, appDirId, false);
						directorExperianConsumerCheck.Execute();
					},
					"GetCustomerDirectorsForConsumerCheck",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);
			} // if

			if (wasMainStrategyExecutedBefore) {
				Library.Instance.Log.Info("Performing experian company check");
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, false);
				experianCompanyChecker.Execute();
			} // if

			if (wasMainStrategyExecutedBefore)
				new AmlChecker(customerId).Execute();

			bool shouldRunBwa =
				appBankAccountType == "Personal" &&
				bwaBusinessCheck == "1" &&
				appSortCode != null &&
				appAccountNumber != null;

			if (shouldRunBwa)
				new BwaChecker(customerId).Execute();

			Library.Instance.Log.Info("Getting Zoopla data for customer {0}", customerId);
			new ZooplaStub(customerId).Execute();
		} // ExecuteAdditionalStrategies

		private readonly CustomerRepository _customers;
		private readonly DecisionHistoryRepository _decisionHistory;
		private readonly DiscountPlanRepository _discountPlanRepository;
		private readonly LoanSourceRepository _loanSourceRepository;
		private readonly LoanTypeRepository _loanTypeRepository;
		private readonly ISession _session;
		private readonly int avoidAutomaticDecision;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly LandRegistryRepository landRegistryRepository;

		// Inputs
		private readonly int customerId;
		private readonly DataGatherer dataGatherer;
		private readonly FinishWizardArgs finishWizardArgs;

		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly MedalScoreCalculator medalScoreCalculator;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly Staller staller;
		private AutoDecisionResponse autoDecisionResponse;

		private bool isFirstLoan;

		// Calculated based on raw data
		private bool isHomeOwner;
		private Medal medalClassification;
		private TurnoverType? turnoverType;
		private decimal medalScore;
		private Ezbob.Backend.Strategies.MedalCalculations.MedalType medalType;
		private int modelLoanOffer;
		private int offeredCreditLine;

		/// <summary>
		///     Default: true. However when Main strategy is executed as a part of
		///     Finish Wizard strategy and customer is already approved/rejected
		///     then customer's status should not change.
		/// </summary>
		private bool overrideApprovedRejected;

		private bool wasMainStrategyExecutedBefore;
	} // class MainStrategy
} // namespace
