namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading.Tasks;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject;
	using Ezbob.Backend.Strategies.Alibaba;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;
	using LandRegistryLib;
	using SalesForceLib.Models;

	public class MainStrategy : AStrategy {
		public MainStrategy(
			int underwriterID,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			FinishWizardArgs fwa,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		) {
			this.underwriterID = underwriterID;
			this.customerId = customerId;
			this.newCreditLineOption = newCreditLine;
			this.avoidAutomaticDecision = avoidAutoDecision;
			this.finishWizardArgs = fwa;
			this.overrideApprovedRejected = true;
			this.cashRequestID = new InternalCashRequestID(cashRequestID);
			this.cashRequestOriginator = cashRequestOriginator;

			if (this.finishWizardArgs != null) {
				this.cashRequestOriginator = this.finishWizardArgs.CashRequestOriginator;
				this.finishWizardArgs.DoMain = false;
			} // if

			this.wasMismatch = false;

			this.backdoorSimpleDetails = null;

			this.autoDecisionResponse = new AutoDecisionResponse();

			this.tag = string.Format(
				"#MainStrategy_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);

			this.customerDetails = new CustomerDetails(this.customerId);

			this.mailer = new StrategiesMailer();
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected) {
			this.overrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		} // SetOverrideApprovedRejected

		public AutoDecisionResponse AutoDecisionResponse {
			get { return this.autoDecisionResponse; }
		} // AutoDecisionResponse

		public override void Execute() {
			ValidateInput();

			if (this.finishWizardArgs != null)
				new FinishWizard(this.finishWizardArgs).Execute();

			CreateCashRequest();

			if (this.newCreditLineOption == NewCreditLineOption.SkipEverything) {
				Log.Debug(
					"Main strategy was activated in 'skip everything go to manual decision mode' for customer {0}.",
					this.customerId
				);

				new SilentAutomation(this.customerId)
					.PreventMainStrategy()
					.SetTag(SilentAutomation.Callers.MainSkipEverything)
					.Execute();

				Log.Debug(
					"Main strategy was activated in 'skip everything go to manual decision mode'." +
					"Nothing more to do for customer id '{0}'. Bye.", this.customerId
				);

				return;
			} // if

			if (this.cashRequestID.LacksValue) { // Should never happen at this point but just in case...
				throw new StrategyAlert(
					this,
					string.Format(
						"No cash request to update for customer {0} (neither specified nor created).",
						this.customerId
					)
				);
			} // if

			bool useStandardFlow = !UseBackdoorSimpleFlow() || !BackdoorSimpleFlow();

			if (useStandardFlow)
				StandardFlow();

			UpdateCustomerAndCashRequest();

			UpdateCustomerAnalyticsLocalData();

			SendEmails();
		} // Execute

		private void StandardFlow() {
			if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules) {
				MarketplaceUpdateStatus mpus = UpdateMarketplaces();

				new Staller(this.customerId, this.mailer)
					.SetMarketplaceUpdateStatus(mpus)
					.Stall();

				ExecuteAdditionalStrategies();
			} // if

			if (!this.customerDetails.IsTest) {
				var fraudChecker = new FraudChecker(this.customerId, FraudMode.FullCheck);
				fraudChecker.Execute();
			} // if

			ForceNhibernateResync.Do(this.customerId);

			ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			GetLandRegistryData();

			CalculateMedal(true);

			if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision) {
				new SilentAutomation(this.customerId)
					.PreventMainStrategy()
					.SetTag(SilentAutomation.Callers.MainUpdateAndGoManual)
					.SetMedal(this.medal)
					.Execute();
			} // if

			CapOffer();

			ProcessApprovals();

			AdjustOfferredCreditLine();
		} // StandardFlow

		/// <summary>
		/// Executes back door simple flow.
		/// This method result is used to determine whether to execute a standard flow.
		/// </summary>
		/// <returns>True if back door flow was executed, false otherwise.</returns>
		private bool BackdoorSimpleFlow() {
			if (this.backdoorSimpleDetails == null)
				return false;

			bool success = this.backdoorSimpleDetails.SetResult(this.autoDecisionResponse);

			if (!success)
				return false;

			CalculateMedal(false);

			if (this.backdoorSimpleDetails.Decision != DecisionActions.Approve)
				return true;

			BackdoorSimpleApprove bsa = this.backdoorSimpleDetails as BackdoorSimpleApprove;

			if (bsa == null) // Should never happen because of the "if" condition.
				return false;

			this.offeredCreditLine = bsa.ApprovedAmount;

			this.medal.MedalClassification = bsa.MedalClassification;
			this.medal.OfferedLoanAmount = bsa.ApprovedAmount;
			this.medal.TotalScoreNormalized = 1m;
			this.medal.AnnualTurnover = bsa.ApprovedAmount;

			return true;
		} // BackdoorSimpleFlow

		private bool UseBackdoorSimpleFlow() {
			if (this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) {
				Log.Debug(
					"Not using back door simple flow for customer '{0}': originator is '{1}'.",
					this.customerId,
					this.cashRequestOriginator == null ? "-- null --" : this.cashRequestOriginator.Value.ToString()
				);
				return false;
			} // if

			if (!this.customerDetails.IsTest) {
				Log.Debug(
					"Not using back door simple flow for customer '{0}': not a text customer.",
					this.customerId
				);
				return false;
			} // if

			this.backdoorSimpleDetails = ABackdoorSimpleDetails.Create(this.customerDetails);

			return this.backdoorSimpleDetails != null;
		} // UseBackdoorSimpleFlow

		private void CalculateMedal(bool updateWasMismatch) {
			var instance = new CalculateMedal(this.customerId, DateTime.UtcNow, false, true);
			instance.Execute();

			if (instance.WasMismatch && updateWasMismatch)
				this.wasMismatch = true;

			this.medal = instance.Result;
		} // CalculateMedal

		private void SendEmails() {
			bool sendToCustomer =
				!this.customerDetails.FilledByBroker || (this.customerDetails.NumOfPreviousApprovals != 0);

			var postMaster = new MainStrategyMails(
				this.mailer,
				this.customerId,
				this.offeredCreditLine,
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
						bool isOwnerAccordingToLandRegistry = LandRegistryRes.IsOwner(
							this.customerDetails.ID,
							this.customerDetails.FullName,
							landRegistryDataModel.Response,
							landRegistryDataModel.Res.TitleNumber
						);

						DB.ExecuteNonQuery(
							"AttachCustomerAddrToLandRegistryAddr",
							CommandSpecies.StoredProcedure,
							new QueryParameter("@LandRegistryAddressID", dbLandRegistry.Id),
							new QueryParameter("@CustomerAddressID", address.AddressId),
							new QueryParameter("@IsOwnerAccordingToLandRegistry", isOwnerAccordingToLandRegistry)
						);
					} // if
				} else {
					int num = 0;

					if (model != null && model.Enquery != null && model.Enquery.Titles != null)
						num = model.Enquery.Titles.Count;

					Log.Warn(
						"No land registry retrieved for customer id: {5}," +
						"house name: {0}, house number: {1}, flat number: {2}, postcode: {3}, # of inquiries {4}",
						address.HouseName,
						address.HouseNumber,
						address.FlatOrApartmentNumber,
						address.PostCode,
						num,
						this.customerId
					);
				} // if
			} // for each
		} // GetLandRegistryData

		private void GetLandRegistryData() {
			bool bSkip =
				this.newCreditLineOption == NewCreditLineOption.SkipEverything ||
				this.newCreditLineOption == NewCreditLineOption.SkipEverythingAndApplyAutoRules;

			if (bSkip)
				return;

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
		} // GetLandRegistryData

		private void ProcessApprovals() {
			bool bContinue = true;

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (this.wasMismatch && bContinue) {
				Log.Info("Not processing approvals: there was a mismatch during rejection.");
				bContinue = false;
			} // if

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
				var raAgent = new AutoDecisionAutomation.AutoDecisions.ReApproval.Agent(
					this.customerId,
					this.cashRequestID,
					DB,
					Log
				).Init();

				raAgent.MakeDecision(this.autoDecisionResponse, this.tag);

				if (raAgent.WasMismatch) {
					this.wasMismatch = true;
					bContinue = false;

					Log.Warn("Mismatch happened while executing re-approval, automation aborted.");
				} else {
					bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

					if (!bContinue)
						Log.Debug("Auto re-approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
				} // if
			} else
				Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");

			if (EnableAutomaticApproval && bContinue) {
				var aAgent = new Approval(
					this.customerId,
					this.cashRequestID,
					this.offeredCreditLine,
					this.medal.MedalClassification,
					(AutomationCalculator.Common.MedalType)this.medal.MedalType,
					(AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
					DB,
					Log
				).Init();

				aAgent.MakeDecision(this.autoDecisionResponse, this.tag);

				if (aAgent.WasMismatch) {
					this.wasMismatch = true;
					bContinue = false;

					Log.Warn("Mismatch happened while executing approval, automation aborted.");
				} else {
					bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

					if (!bContinue)
						Log.Debug("Auto approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
				} // if
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

				Log.Debug("Approval has not reached decision: setting it to be 'waiting for decision'.");
			} // if
		} // ProcessApprovals

		private void ProcessRejections() {
			if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision)
				return;

			if (this.avoidAutomaticDecision == 1)
				return;

			if (EnableAutomaticReRejection) {
				var rrAgent = new ReRejection(this.customerId, this.cashRequestID, DB, Log);
				rrAgent.MakeDecision(this.autoDecisionResponse, this.tag);

				if (rrAgent.WasMismatch) {
					this.wasMismatch = true;
					Log.Warn("Mismatch happened while executing re-rejection, automation aborted.");
					return;
				} // if
			} // if

			if (this.autoDecisionResponse.IsReRejected)
				return;

			if (!EnableAutomaticRejection)
				return;

			if (this.customerDetails.IsAlibaba)
				return;

			var rAgent = new Agent(this.customerId, this.cashRequestID, DB, Log).Init();
			rAgent.MakeDecision(this.autoDecisionResponse, this.tag);

			if (rAgent.WasMismatch) {
				this.wasMismatch = true;
				Log.Warn("Mismatch happened while executing rejection, automation aborted.");
			} // if
		} // ProcessRejections

		/// <summary>
		/// Last stage of auto-decision process
		/// </summary>
		private void UpdateCustomerAndCashRequest() {
			var sp = new MainStrategyUpdateCrC(
				this.customerId,
				this.cashRequestID,
				this.autoDecisionResponse,
				DB,
				Log
			) {
				OverrideApprovedRejected = this.overrideApprovedRejected,
				MedalClassification = this.medal.MedalClassification.ToString(),
				OfferedCreditLine = this.offeredCreditLine,
				SystemCalculatedSum = this.medal.RoundOfferedAmount(),
				TotalScoreNormalized = this.medal.TotalScoreNormalized,
				ExperianConsumerScore = this.customerDetails.ExperianConsumerScore,
				AnnualTurnover = (int)this.medal.AnnualTurnover,
			};

			sp.ExecuteNonQuery();

			//AddNewDecisionOffer(
			//	now,
			//	isLoanTypeSelectionAllowedToUse,
			//	isCustomerRepaymentPeriodSelectionAllowedToUse,
			//	loanTypeIdToUse,
			//	!this.autoDecisionResponse.LoanOfferEmailSendingBannedNew,
			//	discountPlanIDToUse
			//);

			// TODO update new offer / decision tables
			Log.Debug("update new offer / decision for customer {0}", this.customerId);

			// TEMPORARY DISABLED TODO - sync for proper launch
			UpdateSalesForceOpportunity(this.customerDetails.AppEmail);

			if (this.customerDetails.IsAlibaba)
				UpdatePartnerAlibaba(this.customerId);
		} // UpdateCustomerAndCashRequest

		/*
		private void AddNewDecisionOffer(
			DateTime now,
			bool isLoanTypeSelectionAllowed,
			bool isCustomerRepaymentPeriodSelectionAllowed,
			int loanTypeIdToUse,
			bool sendEmailNotification,
			int discountPlanID
		) {
			AddDecision addDecisionStra = new AddDecision(new NL_Decisions {
				DecisionNameID = this.autoDecisionResponse.Decision.HasValue
					? (int)this.autoDecisionResponse.Decision.Value
					: (int)DecisionActions.Waiting,
				DecisionTime = now,
				InterestOnlyRepaymentCount = 0, // TODO
				// TODO ALERT! GEVOLT! FIX! Amount selection allowed IS NOT RELATED to period selection allowed!
				IsAmountSelectionAllowed = isLoanTypeSelectionAllowed, // TODO
				IsRepaymentPeriodSelectionAllowed = isCustomerRepaymentPeriodSelectionAllowed,
				Notes = this.autoDecisionResponse.CreditResult.HasValue
					? this.autoDecisionResponse.CreditResult.Value.DescriptionAttr()
					: "",
				// TODO Position = 
				// TODO CashRequestID = 
				SendEmailNotification = sendEmailNotification,
				UserID = 1,
			}, this.cashRequestID, null);

			addDecisionStra.Execute();
			int decisionID = addDecisionStra.DecisionID;

			int loanSourceID;

			if (this.autoDecisionResponse.LoanSourceID > 0)
				loanSourceID = this.autoDecisionResponse.LoanSourceID;
			else {
				SafeReader lssr = DB.GetFirst("GetDefaultLoanSource", CommandSpecies.StoredProcedure);
				loanSourceID = lssr["LoanSourceID"];
			} // if

			AddOffer addOfferStra = new AddOffer(new NL_Offers {
				DecisionID = decisionID,
				Amount = this.offeredCreditLine,
				// TODO BrokerSetupFeePercent = 0 
				CreatedTime = now,
				DiscountPlanID = discountPlanID,
				EmailSendingBanned = !sendEmailNotification,
				InterestOnlyRepaymentCount = 0, // TODO
				IsLoanTypeSelectionAllowed = isLoanTypeSelectionAllowed,
				LoanSourceID = loanSourceID,
				LoanTypeID = loanTypeIdToUse,
				MonthlyInterestRate = this.autoDecisionResponse.InterestRate,
				RepaymentCount = this.autoDecisionResponse.RepaymentPeriod,
				RepaymentIntervalTypeID = (int)RepaymentIntervalTypesId.Month, // TODO
				SetupFeePercent = this.autoDecisionResponse.SetupFee,
				// DistributedSetupFeePercent TODO EZ-3515
				StartTime = now,
				EndTime = now.AddHours(CurrentValues.Instance.OfferValidForHours)
			});
			addOfferStra.Execute();
			int offerID = addOfferStra.OfferID;
		} // AddNewDecisionOffer
		*/

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

			new ExperianConsumerCheck(this.customerId, null, false)
				.PreventSilentAutomation()
				.Execute();

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
				new ExperianCompanyCheck(this.customerId, false)
					.PreventSilentAutomation()
					.Execute();
			} // if

			if (preData.LastStartedMainStrategyEndTime.HasValue)
				new AmlChecker(this.customerId).PreventSilentAutomation().Execute();

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

		private static bool EnableAutomaticApproval { get { return CurrentValues.Instance.EnableAutomaticApproval; } }
		private static bool EnableAutomaticReApproval { get { return CurrentValues.Instance.EnableAutomaticReApproval; } }
		private static bool EnableAutomaticRejection { get { return CurrentValues.Instance.EnableAutomaticRejection; } }
		private static bool EnableAutomaticReRejection { get { return CurrentValues.Instance.EnableAutomaticReRejection; } }
		private static int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private static int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		/// <summary>
		/// Auto decision only treated 
		/// In case of auto decision occurred (RR, R, RA, A), 002 sent immediately
		/// In the case of "Waiting"/manual, 002 will be transmitted in UI underwrites,
		/// CustomerController, ChangeStatus method.
		/// </summary>
		/// <param name="customerID"></param>
		private void UpdatePartnerAlibaba(int customerID) {
			DecisionActions autoDecision = this.autoDecisionResponse.Decision ?? DecisionActions.Waiting;

			Log.Debug(
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

			default: // unknown auto decision status
				throw new StrategyAlert(
					this,
					string.Format("Auto decision invalid value {0} for customer {1}", autoDecision, customerID)
				);
			} // switch
		} // UpdatePartnerAlibaba

		private void ValidateInput() {
			if ((this.customerDetails.ID <= 0) || (this.customerDetails.ID != this.customerId)) {
				throw new StrategyAlert(
					this,
					string.Format("Customer details were not found for id {0}.", this.customerId)
				);
			} // if

			if (this.cashRequestID.LacksValue) {
				if (this.cashRequestOriginator == null) { // Should never happen but just in case...
					throw new StrategyAlert(
						this,
						string.Format(
							"Neither cash request id nor cash request originator specified for customer {0} " +
							"(cash request cannot be created).",
							this.customerId
						)
					);
				} // if
			} else {
				bool isMatch = DB.ExecuteScalar<bool>(
					"ValidateCustomerAndCashRequest",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerId),
					new QueryParameter("@CashRequestID", this.cashRequestID.Value)
				);

				if (!isMatch) {
					throw new StrategyAlert(
						this,
						string.Format(
							"Cash request id {0} does not belong to customer {1}.",
							this.cashRequestID.Value,
							this.customerId
						)
					);
				} // if
			} // if
		} // ValidateInput

		private void CreateCashRequest() {
			if (this.cashRequestID.HasValue)
				return;

			DateTime now = DateTime.UtcNow;

			SafeReader sr = DB.GetFirst(
				"MainStrategyCreateCashRequest",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerId),
				new QueryParameter("@Now", now),
				// ReSharper disable once PossibleInvalidOperationException
				// This check is done in ValidateInput().
				new QueryParameter("@Originator", this.cashRequestOriginator.Value.ToString())
			);

			if (sr.IsEmpty) {
				throw new StrategyAlert(
					this,
					string.Format("Cash request was not created for customer {0}.", this.customerId)
				);
			} // if

			this.cashRequestID.Value = sr["CashRequestID"];
			decimal? lastLoanAmount = sr["LastLoanAmount"];
			int cashRequestCount = sr["CashRequestCount"];

		/*	new AddCashRequest(new NL_CashRequests {
				CashRequestOriginID = (int)this.cashRequestOriginator.Value,
				CustomerID = this.customerId,
				OldCashRequestID = this.cashRequestID,
				RequestTime = now,
				UserID = this.underwriterID,
			}).Execute(); */

			// TODO add new cash request

			if (this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) {
				new AddOpportunity(this.customerId,
					new OpportunityModel {
						Email = this.customerDetails.AppEmail,
						CreateDate = now,
						ExpectedEndDate = now.AddDays(7),
						RequestedAmount = lastLoanAmount.HasValue ? (int)lastLoanAmount.Value : (int?)null,
						Type = OpportunityType.Resell.DescriptionAttr(),
						Stage = OpportunityStage.s5.DescriptionAttr(),
						Name = this.customerDetails.FullName + cashRequestCount
					}
				).Execute();
			} // if
		} // CreateCashRequest

		private MarketplaceUpdateStatus UpdateMarketplaces() {
			bool updateEverything =
				this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndApplyAutoRules ||
				this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision;

			if (!updateEverything)
				return null;

			Log.Debug("Checking which marketplaces should be updated for customer {0}...", this.customerId);

			DateTime now = DateTime.UtcNow;

			var mpsToUpdate = new List<int>();

			DB.ForEachRowSafe(
				sr => {
					DateTime lastUpdateTime = sr["UpdatingEnd"];

					if ((now - lastUpdateTime).Days > CurrentValues.Instance.UpdateOnReapplyLastDays)
						mpsToUpdate.Add(sr["MpID"]);
				},
				"LoadMarketplacesLastUpdateTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerId)
			);

			if (mpsToUpdate.Count < 1) {
				Log.Debug("No marketplace should be updated for customer {0}.", this.customerId);
				return null;
			} // if

			Log.Debug(
				"{2} to update for customer {0}: {1}.",
				this.customerId,
				string.Join(", ", mpsToUpdate),
				Grammar.Number(mpsToUpdate.Count, "Marketplace")
			);

			var mpus = new MarketplaceUpdateStatus(mpsToUpdate);

			foreach (int mpID in mpsToUpdate) {
				int thisMpID = mpID; // to avoid "Access to foreach variable in closure".

				Task.Run(() => {
					Log.Debug("Updating marketplace {0} for customer {1}...", thisMpID, this.customerId);

					new UpdateMarketplace(this.customerId, thisMpID, false)
						.PreventSilentAutomation()
						.SetMarketplaceUpdateStatus(mpus)
						.Execute();

					Log.Debug("Updating marketplace {0} for customer {1} complete.", thisMpID, this.customerId);
				});
			} // for each

			Log.Debug(
				"Update launched for marketplaces {1} of customer {0}.",
				this.customerId,
				string.Join(", ", mpsToUpdate)
			);

			return mpus;
		} // UpdateMarketplaces

		// Inputs
		private readonly int underwriterID;
		private readonly int customerId;
		private readonly FinishWizardArgs finishWizardArgs;
		private readonly int avoidAutomaticDecision;

		// Helpers
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly AutoDecisionResponse autoDecisionResponse;

		private readonly CustomerDetails customerDetails;

		private MedalResult medal;

		private int offeredCreditLine;

		private class InternalCashRequestID {
			public static implicit operator long(InternalCashRequestID cr) {
				return cr == null ? 0 : cr.Value;
			} // operator long

			public InternalCashRequestID(long? cashRequestID) {
				this.cashRequestID = cashRequestID;
			} // constructor

			public bool HasValue {
				get { return this.cashRequestID.HasValue && (this.cashRequestID.Value > 0); }
			} // HasValue

			public bool LacksValue {
				get { return !HasValue; }
			} // HasValue

			public long Value {
				get { return this.cashRequestID ?? 0; }
				set { this.cashRequestID = value; }
			} // HasValue

			private long? cashRequestID;
		} // class InternalCashRequestID

		private readonly InternalCashRequestID cashRequestID;

		private readonly StrategiesMailer mailer;

		/// <summary>
		/// Default: true. However when Main strategy is executed as a part of
		/// Finish Wizard strategy and customer is already approved/rejected
		/// then customer's status should not change.
		/// </summary>
		private bool overrideApprovedRejected;

		private readonly CashRequestOriginator? cashRequestOriginator;

		private readonly string tag;
		private bool wasMismatch;
		private ABackdoorSimpleDetails backdoorSimpleDetails;
	} // class MainStrategy
} // namespace
