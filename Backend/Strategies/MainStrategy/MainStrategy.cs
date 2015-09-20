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
			int customerID,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			FinishWizardArgs fwa,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		) {
			UnderwriterID = underwriterID;
			this.newCreditLineOption = newCreditLine;
			this.avoidAutomaticDecision = avoidAutoDecision;
			this.finishWizardArgs = fwa;
			this.overrideApprovedRejected = true;
			this.cashRequestID = new InternalCashRequestID(cashRequestID);
			this.cashRequestOriginator = cashRequestOriginator;

			if (this.finishWizardArgs != null) {
				this.cashRequestOriginator = this.finishWizardArgs.CashRequestOriginator;
				this.finishWizardArgs.DoMain = false;
				this.overrideApprovedRejected =
					this.finishWizardArgs.CashRequestOriginator != CashRequestOriginator.Approved;
			} // if

			this.wasMismatch = false;

			this.backdoorSimpleDetails = null;

			this.autoDecisionResponse = new AutoDecisionResponse();

			this.tag = string.Format(
				"#MainStrategy_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);

			this.nlExists = DB.ExecuteScalar<bool>("NL_Exists", CommandSpecies.StoredProcedure);

			this.customerDetails = new CustomerDetails(customerID);

			this.mailer = new StrategiesMailer();
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public int UnderwriterID { get; private set; }

		public int CustomerID {
			get { return this.customerDetails.ID; }
		} // CustomerID

		/// <exception cref="StrategyAlert">Should never happen.</exception>
		public override void Execute() {
			ValidateInput();

			if (this.finishWizardArgs != null)
				new FinishWizard(this.finishWizardArgs).Execute();

			CreateCashRequest();

			this.cashRequestID.Validate(this);

			if (!SkipEverything())
				if (!UseBackdoorSimpleFlow() || !BackdoorSimpleFlow())
					StandardFlow();

			UpdateCustomerAndCashRequest();

			UpdateCustomerAnalyticsLocalData();

			SendEmails();
		} // Execute

		private bool SkipEverything() {
			if (this.newCreditLineOption != NewCreditLineOption.SkipEverything)
				return false;

			Log.Debug(
				"Main strategy was activated in 'skip everything go to manual decision mode'" +
				" for customer {0} by underwriter {1}.",
				CustomerID,
				UnderwriterID
			);

			CalculateMedal(false); 

			new SilentAutomation(CustomerID)
				.PreventMainStrategy()
				.SetTag(SilentAutomation.Callers.MainSkipEverything)
				.SetMedal(this.medal)
				.Execute();

			this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
			this.autoDecisionResponse.UserStatus = Status.Manual;

			Log.Debug(
				"Main strategy was activated in 'skip everything go to manual decision mode' by underwriter '{1}'. " +
				"Nothing more to do for customer id '{0}'. Bye.",
				CustomerID,
				UnderwriterID
			);

			return true;
		} // SkipEverything

		private void StandardFlow() {
			if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules) {
				MarketplaceUpdateStatus mpus = UpdateMarketplaces();

				new Staller(CustomerID, this.mailer)
					.SetMarketplaceUpdateStatus(mpus)
					.Stall();

				ExecuteAdditionalStrategies();
			} // if

			if (!this.customerDetails.IsTest) {
				var fraudChecker = new FraudChecker(CustomerID, FraudMode.FullCheck);
				fraudChecker.Execute();
			} // if

			ForceNhibernateResync.ForCustomer(CustomerID);

			ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			new MainStrategyUpdateLandRegistryData(
				this.customerDetails,
				this.newCreditLineOption,
				this.autoDecisionResponse
			).Execute();

			CalculateMedal(true);

			if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision) {
				new SilentAutomation(CustomerID)
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
			if (!CurrentValues.Instance.BackdoorSimpleAutoDecisionEnabled) {
				Log.Debug("Not using back door simple flow: disabled by configuration (BackdoorSimpleAutoDecisionEnabled).");
				return false;
			} // if

			if (this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) {
				Log.Debug(
					"Not using back door simple flow for customer '{0}': cash request originator is '{1}'.",
					CustomerID,
					this.cashRequestOriginator == null ? "-- null --" : this.cashRequestOriginator.Value.ToString()
				);
				return false;
			} // if

			if (!this.customerDetails.IsTest) {
				Log.Debug(
					"Not using back door simple flow for customer '{0}': not a test customer.",
					CustomerID
				);
				return false;
			} // if

			this.backdoorSimpleDetails = ABackdoorSimpleDetails.Create(this.customerDetails);

			return this.backdoorSimpleDetails != null;
		} // UseBackdoorSimpleFlow

		private void CalculateMedal(bool updateWasMismatch) {
			var instance = new CalculateMedal(CustomerID, this.cashRequestID, DateTime.UtcNow, false, true) {
				Tag = this.tag,
			};
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
				CustomerID,
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
				new QueryParameter("CustomerId", CustomerID),
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
				new QueryParameter("CustomerID", CustomerID),
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
			bool isHomeOwner = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerID)
			);

			this.offeredCreditLine = Math.Min(
				this.medal.RoundOfferedAmount(),
				isHomeOwner ? MaxCapHomeOwner : MaxCapNotHomeOwner
			);

			Log.Debug(
				"Capped for {0}home owner according to land registry: " +
				"capped amount {1} for customer {2} and underwriter {3}.",
				isHomeOwner ? string.Empty : "not ",
				this.offeredCreditLine,
				CustomerID,
				UnderwriterID
			);
		} // CapOffer

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
					CustomerID,
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
					CustomerID,
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
				new BankBasedApproval(CustomerID).MakeDecision(this.autoDecisionResponse);

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
				var rrAgent = new ReRejection(CustomerID, this.cashRequestID, DB, Log);
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

			var rAgent = new Agent(CustomerID, this.cashRequestID, DB, Log).Init();
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
			DateTime now = DateTime.UtcNow;

			AddOldDecisionOffer(now); 

			if (this.nlExists)
				AddNLDecisionOffer(now); 

			UpdateSalesForceOpportunity();

			if (this.customerDetails.IsAlibaba)
				UpdatePartnerAlibaba();
		} // UpdateCustomerAndCashRequest

		private void AddOldDecisionOffer(DateTime now) {
			var sp = new MainStrategyUpdateCrC(
				now,
				CustomerID,
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
		} // AddOldDecisionOffer

		private void AddNLDecisionOffer(DateTime now) {
			if (!this.nlExists)
				return;

			if (!this.autoDecisionResponse.HasAutoDecided)
				return;
			
			AddDecision addDecisionStra = new AddDecision(new NL_Decisions {
				DecisionNameID = this.autoDecisionResponse.DecisionCode ?? (int)DecisionActions.Waiting,
				DecisionTime = now,
				Notes = this.autoDecisionResponse.CreditResult.HasValue
					? this.autoDecisionResponse.CreditResult.Value.DescriptionAttr()
					: string.Empty,
				CashRequestID = this.nlCashRequestID,
				UserID = 1,
			}, this.cashRequestID, null);

			addDecisionStra.Execute();
			int decisionID = addDecisionStra.DecisionID;

			Log.Debug("Added NL decision: {0}", decisionID);

			if (this.autoDecisionResponse.DecidedToApprove) {
				List<NL_OfferFees> offerFees = new List<NL_OfferFees>();

				offerFees.Add(new NL_OfferFees {
					LoanFeeTypeID = (int)FeeTypes.SetupFee,
					Percent = this.autoDecisionResponse.SetupFee,
				});

				AddOffer addOfferStrategy = new AddOffer(new NL_Offers {
					DecisionID = decisionID,
					Amount = this.offeredCreditLine,
					StartTime = now,
					EndTime = now.AddHours(CurrentValues.Instance.OfferValidForHours),
					CreatedTime = now,
					DiscountPlanID = this.autoDecisionResponse.DiscountPlanIDToUse,
					LoanSourceID = this.autoDecisionResponse.LoanSource.ID,
					LoanTypeID = this.autoDecisionResponse.LoanTypeID,
					RepaymentIntervalTypeID = (int)RepaymentIntervalTypesId.Month, // TODO some day...
					MonthlyInterestRate = this.autoDecisionResponse.InterestRate,
					RepaymentCount = this.autoDecisionResponse.RepaymentPeriod,
					BrokerSetupFeePercent = this.autoDecisionResponse.BrokerSetupFeePercent,
					IsLoanTypeSelectionAllowed = this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed,
					IsRepaymentPeriodSelectionAllowed = this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed,
					SendEmailNotification = !this.autoDecisionResponse.LoanOfferEmailSendingBannedNew,
					// ReSharper disable once PossibleInvalidOperationException
					Notes = "Auto decision: " + this.autoDecisionResponse.Decision.Value,
				}, offerFees);

				addOfferStrategy.Execute();
				
				Log.Debug("Added NL offer: {0}", addOfferStrategy.OfferID);
			} // if
		} // AddNLDecisionOffer

		private void UpdateSalesForceOpportunity() {
			string customerEmail = this.customerDetails.AppEmail;

			new AddUpdateLeadAccount(customerEmail, CustomerID, false, false).Execute();

			if (!this.autoDecisionResponse.Decision.HasValue)
				return;

			switch (this.autoDecisionResponse.Decision.Value) {
			case DecisionActions.Approve:
			case DecisionActions.ReApprove:
				new UpdateOpportunity(CustomerID, new OpportunityModel {
					ApprovedAmount = this.autoDecisionResponse.AutoApproveAmount,
					Email = customerEmail,
					ExpectedEndDate = this.autoDecisionResponse.AppValidFor,
					Stage = OpportunityStage.s90.DescriptionAttr(),
				}).Execute();
				break;

			case DecisionActions.Reject:
			case DecisionActions.ReReject:
				new UpdateOpportunity(CustomerID, new OpportunityModel {
					Email = customerEmail,
					DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
					DealLostReason = "Auto " + this.autoDecisionResponse.Decision.Value.ToString(),
					CloseDate = DateTime.UtcNow
				}).Execute();
				break;
			} // switch
		} // UpdateSalesForceOpportunity

		private void ExecuteAdditionalStrategies() {
			var preData = new PreliminaryData(CustomerID);

			new ExperianConsumerCheck(CustomerID, null, false)
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

						var directorExperianConsumerCheck = new ExperianConsumerCheck(CustomerID, appDirId, false);
						directorExperianConsumerCheck.Execute();
					},
					"GetCustomerDirectorsForConsumerCheck",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", CustomerID)
				);
			} // if

			if (preData.LastStartedMainStrategyEndTime.HasValue) {
				Library.Instance.Log.Info("Performing experian company check");
				new ExperianCompanyCheck(CustomerID, false)
					.PreventSilentAutomation()
					.Execute();
			} // if

			if (preData.LastStartedMainStrategyEndTime.HasValue)
				new AmlChecker(CustomerID).PreventSilentAutomation().Execute();

			bool shouldRunBwa =
				preData.AppBankAccountType == "Personal" &&
				preData.BwaBusinessCheck == "1" &&
				preData.AppSortCode != null &&
				preData.AppAccountNumber != null;

			if (shouldRunBwa)
				new BwaChecker(CustomerID).Execute();

			Library.Instance.Log.Info("Getting Zoopla data for customer {0}", CustomerID);
			new ZooplaStub(CustomerID).Execute();
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
		private void UpdatePartnerAlibaba() {
			DecisionActions autoDecision = this.autoDecisionResponse.Decision ?? DecisionActions.Waiting;

			Log.Debug(
				"UpdatePartnerAlibaba ******************************************************{0}, {1}",
				CustomerID,
				autoDecision
			);

			//	Reject, Re-Reject, Re-Approve, Approve: 0001 + 0002 (auto decision is a final also)
			// other: 0001 
			switch (autoDecision) {
			case DecisionActions.ReReject:
			case DecisionActions.Reject:
			case DecisionActions.ReApprove:
			case DecisionActions.Approve:
				new DataSharing(CustomerID, AlibabaBusinessType.APPLICATION).Execute();
				new DataSharing(CustomerID, AlibabaBusinessType.APPLICATION_REVIEW).Execute();
				break;

			// auto not final
			case DecisionActions.Waiting:
				new DataSharing(CustomerID, AlibabaBusinessType.APPLICATION).Execute();
				break;

			default: // unknown auto decision status
				throw new StrategyAlert(
					this,
					string.Format("Auto decision invalid value {0} for customer {1}", autoDecision, CustomerID)
				);
			} // switch
		} // UpdatePartnerAlibaba

		private void ValidateInput() {
			if (!this.customerDetails.IsValid) {
				throw new StrategyAlert(
					this,
					string.Format("Customer details were not found for id {0}.", this.customerDetails.RequestedID)
				);
			} // if

			if (this.cashRequestID.LacksValue) {
				if (this.cashRequestOriginator == null) { // Should never happen but just in case...
					throw new StrategyAlert(
						this,
						string.Format(
							"Neither cash request id nor cash request originator specified for customer {0} " +
							"(cash request cannot be created).",
							CustomerID
						)
					);
				} // if
			} else {
				bool isMatch = DB.ExecuteScalar<bool>(
					"ValidateCustomerAndCashRequest",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", CustomerID),
					new QueryParameter("@CashRequestID", this.cashRequestID.Value)
				);

				if (!isMatch) {
					throw new StrategyAlert(
						this,
						string.Format(
							"Cash request id {0} does not belong to customer {1}.",
							this.cashRequestID.Value,
							CustomerID
						)
					);
				} // if
			} // if
		} // ValidateInput

		private void CreateCashRequest() {
			if (this.cashRequestID.HasValue) {
				DB.ExecuteNonQuery(
					"MainStrategySetCustomerIsBeingProcessed",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", CustomerID)
				);

				if (this.nlExists) {
					this.nlCashRequestID = DB.ExecuteScalar<int>(
						"NL_CashRequestGetByOldID",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@OldCashRequestID", this.cashRequestID)
					);
				} // if

				return;
			} // if

			DateTime now = DateTime.UtcNow;

			SafeReader sr = DB.GetFirst(
				"MainStrategyCreateCashRequest",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", CustomerID),
				new QueryParameter("@Now", now),
				// ReSharper disable once PossibleInvalidOperationException
				// This check is done in ValidateInput().
				new QueryParameter("@Originator", this.cashRequestOriginator.Value.ToString())
			);

			if (sr.IsEmpty) {
				throw new StrategyAlert(
					this,
					string.Format("Cash request was not created for customer {0}.", CustomerID)
				);
			} // if

			this.cashRequestID.Value = sr["CashRequestID"];

			if (this.nlExists) {
				AddCashRequest cashRequestStrategy = new AddCashRequest(new NL_CashRequests {
					CashRequestOriginID = (int)this.cashRequestOriginator.Value,
					CustomerID = CustomerID,
					OldCashRequestID = this.cashRequestID,
					RequestTime = now,
					UserID = UnderwriterID,
				});
				cashRequestStrategy.Execute();
				this.nlCashRequestID = cashRequestStrategy.CashRequestID;

				Log.Debug("Added NL CashRequest: {0}", this.nlCashRequestID);
			} // if

			if (this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) {
				decimal? lastLoanAmount = sr["LastLoanAmount"];
				int cashRequestCount = sr["CashRequestCount"];

				new AddOpportunity(CustomerID,
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

			Log.Debug("Checking which marketplaces should be updated for customer {0}...", CustomerID);

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
				new QueryParameter("@CustomerID", CustomerID)
			);

			if (mpsToUpdate.Count < 1) {
				Log.Debug("No marketplace should be updated for customer {0}.", CustomerID);
				return null;
			} // if

			Log.Debug(
				"{2} to update for customer {0}: {1}.",
				CustomerID,
				string.Join(", ", mpsToUpdate),
				Grammar.Number(mpsToUpdate.Count, "Marketplace")
			);

			var mpus = new MarketplaceUpdateStatus(mpsToUpdate);

			foreach (int mpID in mpsToUpdate) {
				int thisMpID = mpID; // to avoid "Access to foreach variable in closure".

				Task.Run(() => {
					Log.Debug("Updating marketplace {0} for customer {1}...", thisMpID, CustomerID);

					new UpdateMarketplace(CustomerID, thisMpID, false)
						.PreventSilentAutomation()
						.SetMarketplaceUpdateStatus(mpus)
						.Execute();

					Log.Debug("Updating marketplace {0} for customer {1} complete.", thisMpID, CustomerID);
				});
			} // for each

			Log.Debug(
				"Update launched for marketplaces {1} of customer {0}.",
				CustomerID,
				string.Join(", ", mpsToUpdate)
			);

			return mpus;
		} // UpdateMarketplaces

		private readonly FinishWizardArgs finishWizardArgs;
		private readonly int avoidAutomaticDecision;

		private readonly NewCreditLineOption newCreditLineOption;
		private readonly AutoDecisionResponse autoDecisionResponse;

		private readonly CustomerDetails customerDetails;

		private MedalResult medal;

		private int offeredCreditLine;

		private readonly InternalCashRequestID cashRequestID;
		private int nlCashRequestID;

		private readonly StrategiesMailer mailer;

		/// <summary>
		/// Default: true. However when Main strategy is executed as a part of
		/// Finish Wizard strategy and customer is already approved/rejected
		/// then customer's status should not change.
		/// </summary>
		private readonly bool overrideApprovedRejected;

		private readonly CashRequestOriginator? cashRequestOriginator;

		private readonly bool nlExists;
		private readonly string tag;
		private bool wasMismatch;
		private ABackdoorSimpleDetails backdoorSimpleDetails;
	} // class MainStrategy
} // namespace
