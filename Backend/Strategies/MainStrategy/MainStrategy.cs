﻿namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Web;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Investor;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;
	using LandRegistryLib;
	using MailApi;
	using SalesForceLib.Models;

	public class MainStrategy : AMainStrategyBase {
		public MainStrategy(
			int underwriterID,
			int customerID,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			FinishWizardArgs fwa,
			long? cashRequestID, // When old cash request is removed replace this with NLcashRequestID
			CashRequestOriginator? cashRequestOriginator
		) {
			this.autoRejectionOutput = null;

			UnderwriterID = underwriterID;
			this.newCreditLineOption = newCreditLine;
			this.avoidAutomaticDecision = (avoidAutoDecision == 1) || newCreditLine.AvoidAutoDecision();
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
				Guid.NewGuid().ToString().ToUpperInvariant()
			);

			this.customerDetails = new CustomerDetails(customerID);
			this.customerDetails.Load();

			this.mailer = new StrategiesMailer();
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public int UnderwriterID { get; private set; }

		public override int CustomerID {
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

			SaveDecision();

			DispatchNotifications();
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
			LoadCompanyAndMonthlyPayment(DateTime.UtcNow);

			if (this.newCreditLineOption.UpdateData()) {
				MarketplaceUpdateStatus mpus = UpdateMarketplaces();

				new Staller(CustomerID, this.mailer)
					.SetMarketplaceUpdateStatus(mpus)
					.Stall();

				ExecuteAdditionalStrategies();
			} // if

			if (!this.customerDetails.IsTest)
				new FraudChecker(CustomerID, FraudMode.FullCheck).Execute();

			ForceNhibernateResync.ForCustomer(CustomerID);

			if (!ProcessRejections())
				return;

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

			this.medal.MedalClassification = bsa.MedalClassification;
			this.medal.OfferedLoanAmount = bsa.ApprovedAmount;
			this.medal.TotalScoreNormalized = 1m;
			this.medal.AnnualTurnover = bsa.ApprovedAmount;

			var glcd = new GetLoanCommissionDefaults(this.cashRequestID, bsa.ApprovedAmount);
			glcd.Execute();

			if (!glcd.IsBrokerCustomer)
				return true;

			this.autoDecisionResponse.BrokerSetupFeePercent = glcd.Result.BrokerCommission;
			this.autoDecisionResponse.SetupFee = glcd.Result.ManualSetupFee;

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
			var instance = new CalculateMedal(
				CustomerID,
				this.cashRequestID,
				this.nlCashRequestID,
				DateTime.UtcNow,
				false,
				true
			) {
				Tag = this.tag,
			};
			instance.Execute();

			if (instance.WasMismatch && updateWasMismatch)
				this.wasMismatch = true;

			this.medal = instance.Result;
		} // CalculateMedal

		private void DispatchNotifications() {
			bool sendToCustomer =
				!this.customerDetails.FilledByBroker || (this.customerDetails.NumOfPreviousApprovals != 0);

			var notifier = new ExternalNotifier(
				this.mailer,
				CustomerID,
				this.medal,
				this.customerDetails,
				this.autoDecisionResponse,
				sendToCustomer,
				Log
			);

			notifier.Execute();
		} // DispatchNotifications

		private void CapOffer() {
			bool isHomeOwner = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerID)
			);

			this.autoDecisionResponse.ProposedAmount = Math.Min(
				this.medal.RoundOfferedAmount(),
				isHomeOwner ? MaxCapHomeOwner : MaxCapNotHomeOwner
			);

			Log.Debug(
				"Capped for {0}home owner according to land registry: " +
				"capped amount {1} for customer {2} and underwriter {3}.",
				isHomeOwner ? string.Empty : "not ",
				this.autoDecisionResponse.ProposedAmount,
				CustomerID,
				UnderwriterID
			);
		} // CapOffer

		private bool ApprovalPreconditions() {
			if (this.wasMismatch) {
				Log.Info("Not processing approvals: there was a mismatch during rejection.");
				return false;
			} // if

			if (this.autoDecisionResponse.DecidedToReject) {
				Log.Info("Not processing approvals: reject decision has been made.");
				return false;
			} // if

			if (this.avoidAutomaticDecision) {
				Log.Info("Not processing approvals: automatic decisions should be avoided.");
				return false;
			} // if

			if (!this.customerDetails.CustomerStatusIsEnabled) {
				Log.Info("Not processing approvals: customer status is not enabled.");
				return false;
			} // if

			if (this.customerDetails.CustomerStatusIsWarning) {
				Log.Info("Not processing approvals: customer status is 'warning'.");
				return false;
			} // if

			if (!EnableAutomaticReRejection) {
				Log.Info("Not processing approvals: auto re-rejection is disabled.");
				return false;
			} // if

			if (!EnableAutomaticRejection) {
				Log.Info("Not processing approvals: auto rejection is disabled.");
				return false;
			} // if

			return true;
		} // ApprovalPreconditions

		private void ProcessApprovals() {
			if (!ApprovalPreconditions())
				return;

			if (DoAutoReapproval())
				return;

			if (this.customerDetails.IsAlibaba) {
				Log.Info("Not processing auto-approval: Alibaba customer.");
				return;
			} // if

			DoAutoApproval();

			if (!this.autoDecisionResponse.SystemDecision.HasValue) { // No decision is made so far
				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;

				Log.Debug("Approval has not reached decision: setting it to be 'waiting for decision'.");
			} // if
		} // ProcessApprovals

		/// <summary>
		/// Executes auto re-approval.
		/// </summary>
		/// <returns>True, if auto re-approval reached some decision (approval or mismatch); false, otherwise.</returns>
		private bool DoAutoReapproval() {
			if (EnableAutomaticReApproval) {
				Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");
				return false;
			} // if

			var raAgent = new AutoDecisionAutomation.AutoDecisions.ReApproval.Agent(
				CustomerID,
				this.cashRequestID,
				this.nlCashRequestID,
				DB,
				Log
			).Init();

			raAgent.MakeAndVerifyDecision(this.tag);

			if (raAgent.ExceptionWhileDeciding) {
				Log.Warn("Exception happened while executing re-approval, auto-decision process aborted.");
				return true;
			} // if exception

			if (raAgent.WasMismatch) {
				this.wasMismatch = true;
				Log.Warn("Mismatch happened while executing re-approval, auto-decision process aborted.");
				return true;
			} // if mismatch

			if (!raAgent.Trail.HasDecided) {
				Log.Debug("Auto re-approval has not reached 'approved' decision, auto-decision process continues.");
				return false;
			} // if

			this.autoDecisionResponse.HasApprovalChance = true;
			this.autoDecisionResponse.ApprovedAmount = raAgent.ApprovedAmount;
			this.autoDecisionResponse.Decision = DecisionActions.ReApprove;
			this.autoDecisionResponse.CreditResult = CreditResultStatus.Approved;
			this.autoDecisionResponse.UserStatus = Status.Approved;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Approve;
			this.autoDecisionResponse.LoanOfferUnderwriterComment = "Auto Re-Approval";
			this.autoDecisionResponse.DecisionName = "Re-Approval";
			this.autoDecisionResponse.AppValidFor = raAgent.AppValidFor;
			this.autoDecisionResponse.LoanOfferEmailSendingBannedNew = raAgent.IsEmailSendingBanned;

			var sr = DB.GetFirst(
				"RapprovalGetLastApproveTerms",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LacrID", raAgent.LastApprovedCashRequestID)
			);

			this.autoDecisionResponse.InterestRate = sr["InterestRate"];
			this.autoDecisionResponse.RepaymentPeriod = sr["RepaymentPeriod"];
			this.autoDecisionResponse.SetupFee = sr["ManualSetupFeePercent"];
			this.autoDecisionResponse.LoanTypeID = sr["LoanTypeID"];
			this.autoDecisionResponse.LoanSourceID = sr["LoanSourceID"];
			this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed =
				sr["IsCustomerRepaymentPeriodSelectionAllowed"];
			this.autoDecisionResponse.BrokerSetupFeePercent = sr["BrokerSetupFeePercent"];
			this.autoDecisionResponse.SpreadSetupFee = sr["SpreadSetupFee"];

			return true;
		} // DoAutoReapproval

		private void DoAutoApproval() {
			if (!EnableAutomaticApproval) {
				Log.Debug(
					"Not processed auto approval: " +
						"it is currently disabled in configuration or decision has already been made earlier."
				);

				return;
			} // if

			if (this.autoRejectionOutput == null) {
				Log.Info(
					"Not processing auto-approval: no auto-rejection output detected (auto rejection did not run?)."
				);

				return;
			} // if

			var aAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.LogicalGlue.Agent(
				new AutoApprovalArguments(
					CustomerID,
					this.cashRequestID,
					this.nlCashRequestID,
					this.autoDecisionResponse.ProposedAmount,
					(AutomationCalculator.Common.Medal)this.medal.MedalClassification,
					(AutomationCalculator.Common.MedalType)this.medal.MedalType,
					(AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
					this.autoRejectionOutput.FlowType,
					this.autoRejectionOutput.ErrorInLGData,
					this.tag,
					DateTime.UtcNow,
					DB,
					Log
				)
			).Init();

			this.autoDecisionResponse.LoanOfferUnderwriterComment = "Checking auto approve...";

			aAgent.MakeAndVerifyDecision();

			if (aAgent.ExceptionWhileDeciding) {
				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Exception - " + aAgent.Trail.UniqueID;

				Log.Alert(
					"Switching to manual decision: exception during  Auto Approval for customer {0}, trail id is {1}.",
					CustomerID,
					aAgent.Trail.UniqueID.ToString().ToUpperInvariant()
				);

				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
				return;
			} // if exception
			
			if (aAgent.WasMismatch) {
				this.wasMismatch = true;

				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Mismatch - " + aAgent.Trail.UniqueID;

				Log.Alert(
					"Switching to manual decision: Auto Approval implementations " +
					"have not reached the same decision for customer {0}, trail id is {1}.",
					CustomerID,
					aAgent.Trail.UniqueID.ToString().ToUpperInvariant()
				);

				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
				return;
			} // if mismatch

			this.autoDecisionResponse.LoanOfferUnderwriterComment =
				aAgent.Trail.GetDecisionName() +
				" - " +
				aAgent.Trail.UniqueID;

			Log.Msg(
				"Both Auto Approval implementations have reached the same decision: {0}approved",
				aAgent.Trail.HasDecided ? string.Empty : "not "
			);

			if (!aAgent.Trail.HasDecided)
				return;

			this.autoDecisionResponse.ApprovedAmount = aAgent.Trail.RoundedAmount;

			try {
				CreateOffer(aAgent);
			} catch (Exception e) {
				Log.Alert(e, "Exception during creating an offer for customer {0}.", CustomerID);
				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Exception in offer - " + aAgent.Trail.UniqueID;
			} // try
		} // DoAutoApproval

		private void CreateOffer(ICreateOfferInputData offerInputData) {
			ApprovalTrail approvalTrail = offerInputData.Trail;

			Tuple<OfferResult, int> offer = offerInputData.LogicalGlueFlowFollowed
				? CreateLogicalOffer()
				: CreateUnlogicalOffer();

			OfferResult offerResult = offer.Item1;
			int loanSourceID = offer.Item2;

			if (offerResult == null || offerResult.IsError) {
				Log.Alert(
					"Customer {1} - will use manual. Offer result: {0}",
					offerResult != null ? offerResult.Description : "",
					CustomerID
				);

				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Calculator failure - " + approvalTrail.UniqueID;
				return;
			} // if

			if (CurrentValues.Instance.AutoApproveIsSilent) {
				this.autoDecisionResponse.HasApprovalChance = true;
				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Silent Approve - " + approvalTrail.UniqueID;

				NotifyAutoApproveSilentMode(
					this.autoDecisionResponse.ApprovedAmount,
					offerResult.Period,
					offerResult.InterestRate / 100m,
					offerResult.SetupFee / 100m,
					approvalTrail
				);

				return;
			} // if

			this.autoDecisionResponse.AppValidFor = DateTime.UtcNow.AddDays(approvalTrail.MyInputData.MetaData.OfferLength);

			var loti = new LinkOfferToInvestor(CustomerID, this.cashRequestID, false, null, UnderwriterID);
			loti.Execute();

			bool investorFound = !loti.IsForOpenPlatform || loti.FoundInvestor;

			if (!investorFound) {
				Log.Info("Customer {0} - will use manual because investor was not found.", CustomerID);

				this.autoDecisionResponse.CreditResult = CreditResultStatus.PendingInvestor;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Investor not found - " + approvalTrail.UniqueID;
			} else {
				this.autoDecisionResponse.HasApprovalChance = true;
				this.autoDecisionResponse.CreditResult = CreditResultStatus.Approved;
				this.autoDecisionResponse.UserStatus = Status.Approved;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Approve;
				this.autoDecisionResponse.LoanOfferUnderwriterComment = "Auto Approval";

				this.autoDecisionResponse.DecisionName = "Approval";
				this.autoDecisionResponse.Decision = DecisionActions.Approve;
				this.autoDecisionResponse.LoanOfferEmailSendingBannedNew =
					approvalTrail.MyInputData.MetaData.IsEmailSendingBanned;

				// Use offer calculated data
				this.autoDecisionResponse.RepaymentPeriod = offerResult.Period;
				this.autoDecisionResponse.LoanSourceID = loanSourceID;
				this.autoDecisionResponse.LoanTypeID = offerResult.LoanTypeId;
				this.autoDecisionResponse.InterestRate = offerResult.InterestRate / 100M;
				this.autoDecisionResponse.SetupFee = offerResult.SetupFee / 100M;
			} // if
		} // CreateOffer

		private Tuple<OfferResult, int> CreateLogicalOffer() {
			this.autoDecisionResponse.ProductSubTypeID = this.autoRejectionOutput.ProductSubTypeID;

			SafeReader sr = DB.GetFirst(
				"LoadGradeRangeAndSubproduct",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@GradeRangeID", this.autoRejectionOutput.GradeRangeID),
				new QueryParameter("@ProductSubTypeID", this.autoRejectionOutput.ProductSubTypeID)
			);

			if (sr.IsEmpty) {
				Log.Alert(
					"Failed to load grade range and product subtype by grade range id {0} and product sub type id {1}.",
					this.autoRejectionOutput.GradeRangeID,
					this.autoRejectionOutput.ProductSubTypeID
				);

				return new Tuple<OfferResult, int>(null, 0);
			} // if

			GradeRangeSubproduct grsp = sr.Fill<GradeRangeSubproduct>();

			int amount = grsp.LoanAmount(MonthlyRepayment.RequestedAmount);

			int maxAmount = CurrentValues.Instance.AutoApproveMaxAmount;
			int minAmount = CurrentValues.Instance.MinLoan;

			if ((amount < minAmount) || (amount > maxAmount)) {
				Log.Msg(
					"Switching to manual: approved amount {0} for customer {1} " +
					"is out of allowed for auto approve range [{2} , {3}].",
					amount.ToString("C0"),
					CustomerID,
					minAmount.ToString("C0"),
					maxAmount.ToString("C0")
				);

				return new Tuple<OfferResult, int>(null, 0);
			} // if

			var offerResult = new OfferResult {
				CustomerId = CustomerID,
				CalculationTime = DateTime.UtcNow,
				Amount = amount,
				MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification,

				ScenarioName = "Logical Glue",
				Period = grsp.Term(MonthlyRepayment.RequestedTerm),
				LoanTypeId = grsp.LoanTypeID,
				LoanSourceId = grsp.LoanSourceID,
				InterestRate = grsp.InterestRate * 100M,
				SetupFee = grsp.SetupFee * 100M,
				Message = null,
				IsError = false,
				IsMismatch = false,
				HasDecision = true,
			};

			return new Tuple<OfferResult, int>(offerResult, grsp.LoanSourceID);
		} // CreateLogicalOffer

		private Tuple<OfferResult, int> CreateUnlogicalOffer() {
			this.autoDecisionResponse.ProductSubTypeID = null;

			SafeReader sr = DB.GetFirst("GetDefaultLoanSource", CommandSpecies.StoredProcedure);

			if (sr.IsEmpty)
				throw new Exception("Failed to detect default loan source.");

			int loanCount = DB.ExecuteScalar<int>(
				"GetCustomerLoanCount",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", CustomerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			int loanSourceID = sr["LoanSourceID"];
			int repaymentPeriod = sr["RepaymentPeriod"] ?? 15;

			var offerDualCalculator = new OfferDualCalculator(
				CustomerID,
				DateTime.UtcNow,
				this.autoDecisionResponse.ProposedAmount,
				loanCount > 0,
				this.medal.MedalClassification,
				loanSourceID,
				repaymentPeriod
			);

			return new Tuple<OfferResult, int>(offerDualCalculator.CalculateOffer(), loanSourceID);
		} // CreateUnlogicalOffer

		/// <summary>
		/// 
		/// </summary>
		/// <returns>True, if everything is ok (mismatch is ok for this matter); false, otherwise.</returns>
		private bool ProcessRejections() {
			if (this.avoidAutomaticDecision) {
				Log.Debug("Not processing auto-rejections: auto decisions should be avoided.");
				return true;
			} // if

			if (EnableAutomaticReRejection) {
				var rrAgent = new ReRejection(CustomerID, this.cashRequestID, this.nlCashRequestID, DB, Log);
				rrAgent.MakeAndVerifyDecision(this.tag);

				if (rrAgent.ExceptionDuringRerejection) {
					Log.Warn("Exception happened while executing re-rejection, auto-decision process aborted.");
					return false;
				} // if

				if (rrAgent.WasMismatch) {
					this.wasMismatch = true;
					Log.Warn("Mismatch happened while executing re-rejection, auto-decision process aborted.");
					return true;
				} // if

				if (rrAgent.Trail.HasDecided) {
					this.autoDecisionResponse.Decision = DecisionActions.ReReject;
					this.autoDecisionResponse.AutoRejectReason = "Auto Re-Reject";
					this.autoDecisionResponse.CreditResult = CreditResultStatus.Rejected;
					this.autoDecisionResponse.UserStatus = Status.Rejected;
					this.autoDecisionResponse.SystemDecision = SystemDecision.Reject;
					this.autoDecisionResponse.DecisionName = "Re-rejection";
				} // if
			} // if

			if (this.autoDecisionResponse.IsReRejected) {
				Log.Debug("Not processing auto-rejection: re-rejected.");
				return true;
			} // if

			if (!EnableAutomaticRejection) {
				Log.Debug("Not processing auto-rejection: auto-rejection is disabled.");
				return true;
			} // if

			if (this.customerDetails.IsAlibaba) {
				Log.Debug("Not processing auto-rejection: Alibaba customer.");
				return true;
			} // if

			var rAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue.Agent(
				new AutoRejectionArguments(
					CustomerID,
					CompanyID,
					MonthlyRepayment.MonthlyPayment,
					this.cashRequestID,
					this.nlCashRequestID,
					this.tag,
					DateTime.UtcNow,
					DB,
					Log
				)
			);

			rAgent.MakeAndVerifyDecision();

			if (rAgent.ExceptionDuringRejection) {
				Log.Warn("Exception happened while executing rejection, auto-decision process aborted.");
				return false;
			} // if

			if (rAgent.WasMismatch) {
				this.wasMismatch = true;
				Log.Warn("Mismatch happened while executing rejection, auto-decision process aborted.");
			} else {
				if (rAgent.Trail.HasDecided) {
					this.autoDecisionResponse.CreditResult = CreditResultStatus.Rejected;
					this.autoDecisionResponse.UserStatus = Status.Rejected;
					this.autoDecisionResponse.SystemDecision = SystemDecision.Reject;
					this.autoDecisionResponse.DecisionName = "Rejection";
					this.autoDecisionResponse.Decision = DecisionActions.Reject;
				} // if

				this.autoRejectionOutput = rAgent.Output;
			} // if

			return true;
		} // ProcessRejections

		private void SaveDecision() {
			DateTime now = DateTime.UtcNow;

			AddOldDecisionOffer(now);

			AddNLDecisionOffer(now);
		} // SaveDecision

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
				OfferedCreditLine = this.autoDecisionResponse.ApprovedAmount,
				SystemCalculatedSum = this.medal.RoundOfferedAmount(),
				TotalScoreNormalized = this.medal.TotalScoreNormalized,
				ExperianConsumerScore = this.customerDetails.ExperianConsumerScore,
				AnnualTurnover = (int)this.medal.AnnualTurnover,
			};

			sp.ExecuteNonQuery();
		} // AddOldDecisionOffer

		private void AddNLDecisionOffer(DateTime now) {
			if (!this.autoDecisionResponse.HasAutoDecided)
				return;

			AddDecision addDecisionStra = new AddDecision(new NL_Decisions {
				DecisionNameID = this.autoDecisionResponse.DecisionCode ?? (int)DecisionActions.Waiting,
				DecisionTime = now,
				Notes = this.autoDecisionResponse.CreditResult.HasValue
					? this.autoDecisionResponse.CreditResult.Value.DescriptionAttr()
					: string.Empty,
				CashRequestID = this.nlCashRequestID,
				UserID = UnderwriterID,
			}, this.cashRequestID, null);

			addDecisionStra.Execute();
			long decisionID = addDecisionStra.DecisionID;

			Log.Debug("Added NL decision: {0}", decisionID);

			if (this.autoDecisionResponse.DecidedToApprove) {
				NL_OfferFees setupFee = new NL_OfferFees {
					LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
					Percent = this.autoDecisionResponse.SetupFee,
					OneTimePartPercent = 1,
					DistributedPartPercent = 0
				};

				if (this.autoDecisionResponse.SpreadSetupFee) {
					setupFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
					setupFee.OneTimePartPercent = 0;
					setupFee.DistributedPartPercent = 1;
				} // if

				NL_OfferFees[] ofeerFees = { setupFee };

				AddOffer addOfferStrategy = new AddOffer(new NL_Offers {
					DecisionID = decisionID,
					Amount = this.autoDecisionResponse.ApprovedAmount,
					StartTime = now,
					EndTime = now.AddHours(CurrentValues.Instance.OfferValidForHours),
					CreatedTime = now,
					DiscountPlanID = this.autoDecisionResponse.DiscountPlanIDToUse,
					LoanSourceID = this.autoDecisionResponse.LoanSource.ID,
					LoanTypeID = this.autoDecisionResponse.LoanTypeID,
					RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month, 
					MonthlyInterestRate = this.autoDecisionResponse.InterestRate,
					RepaymentCount = this.autoDecisionResponse.RepaymentPeriod,
					BrokerSetupFeePercent = this.autoDecisionResponse.BrokerSetupFeePercent,
					IsLoanTypeSelectionAllowed = this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed,
					IsRepaymentPeriodSelectionAllowed = this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed,
					SendEmailNotification = !this.autoDecisionResponse.LoanOfferEmailSendingBannedNew,
					// ReSharper disable once PossibleInvalidOperationException
					Notes = "Auto decision: " + this.autoDecisionResponse.Decision.Value,
				}, ofeerFees);

				addOfferStrategy.Execute();

				Log.Debug("Added NL offer: {0}", addOfferStrategy.OfferID);
			} // if
		} // AddNLDecisionOffer

		private void DoConsumerCheck(PreliminaryData preData) {
			new ExperianConsumerCheck(CustomerID, null, false)
				.PreventSilentAutomation()
				.Execute();

			if (preData.TypeOfBusiness != TypeOfBusiness.Entrepreneur) {
				Strategies.Library.Instance.DB.ForEachRowSafe(
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
		} // DoConsumerCheck

		private void DoCompanyCheck(PreliminaryData preData) {
			if (preData.LastStartedMainStrategyEndTime.HasValue) {
				Strategies.Library.Instance.Log.Info("Performing experian company check");
				new ExperianCompanyCheck(CustomerID, false)
					.PreventSilentAutomation()
					.Execute();
			} // if
		} // DoCompanyCheck

		private void DoAmlCheck(PreliminaryData preData) {
			if (preData.LastStartedMainStrategyEndTime.HasValue)
				new AmlChecker(CustomerID).PreventSilentAutomation().Execute();
		} // DoAmlCheck

		private void DoBwaCheck(PreliminaryData preData) {
			bool shouldRunBwa =
				preData.AppBankAccountType == "Personal" &&
				preData.BwaBusinessCheck == "1" &&
				preData.AppSortCode != null &&
				preData.AppAccountNumber != null;

			if (shouldRunBwa)
				new BwaChecker(CustomerID).Execute();
		} // DoBwaCheck

		private void DoZooplaCheck() {
			Strategies.Library.Instance.Log.Info("Getting Zoopla data for customer {0}", CustomerID);
			new ZooplaStub(CustomerID).Execute();
		} // DoZooplaCheck

		private void UpdateLogicalGlue(PreliminaryData preData) {
			if (!CurrentValues.Instance.LogicalGlueEnabled) {
				Log.Debug("Not updating Logical Glue data: updating disabled.");
				return;
			} // if

			if (preData.TypeOfBusiness.IsRegulated()) {
				Log.Debug("Not updating Logical Glue data: customer {0} has a regulated company.", CustomerID);
				return;
			} // if

			Log.Debug("Updating Logical Glue data: customer {0} has a non-regulated company.", CustomerID);

			try {
				InjectorStub.GetEngine().GetInference(
					CustomerID,
					MonthlyRepayment.MonthlyPayment,
					false,
					GetInferenceMode.DownloadIfOld
				);
				Log.Debug("Updated Logical Glue data for customer {0}.", CustomerID);
			} catch (Exception e) {
				Log.Warn(e, "Logical Glue data was not updated for customer {0}.", CustomerID);
			} // try
		} // UpdateLogicalGlue

		private void ExecuteAdditionalStrategies() {
			var preData = new PreliminaryData(CustomerID);

			DoConsumerCheck(preData);
			DoCompanyCheck(preData);
			DoAmlCheck(preData);
			DoBwaCheck(preData);
			DoZooplaCheck();
			UpdateLogicalGlue(preData); // Must be after DoCompanyCheck because uses ExperianRefNum.
		} // ExecuteAdditionalStrategies

		private static bool EnableAutomaticApproval { get { return CurrentValues.Instance.EnableAutomaticApproval; } }
		private static bool EnableAutomaticReApproval { get { return CurrentValues.Instance.EnableAutomaticReApproval; } }
		private static bool EnableAutomaticRejection { get { return CurrentValues.Instance.EnableAutomaticRejection; } }
		private static bool EnableAutomaticReRejection { get { return CurrentValues.Instance.EnableAutomaticReRejection; } }
		private static int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private static int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

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
			DateTime now = DateTime.UtcNow;

			if (this.cashRequestID.HasValue) {
				DB.ExecuteNonQuery(
					"MainStrategySetCustomerIsBeingProcessed",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", CustomerID)
				);

				this.nlCashRequestID = DB.ExecuteScalar<long>(
					"NL_CashRequestGetByOldID",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@OldCashRequestID", this.cashRequestID.Value)
				);

				if (this.nlCashRequestID == 0L)
					NLCashRequestAdd(this.cashRequestID.Value, now);

				return;
			} // if

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
				throw new StrategyAlert(this, string.Format("Cash request was not created for customer {0}.", CustomerID)
				);
			} // if

			this.cashRequestID.Value = sr["CashRequestID"];

			NLCashRequestAdd(this.cashRequestID.Value, now);

			int cashRequestCount = sr["CashRequestCount"];

			bool addOpportunity = 
				(this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) &&
				(this.cashRequestOriginator != CashRequestOriginator.ForcedWizardCompletion) &&
				(cashRequestCount > 1);

			if (addOpportunity) {
				decimal? lastLoanAmount = sr["LastLoanAmount"];

				new AddOpportunity(CustomerID,
					new OpportunityModel {
						Email = this.customerDetails.AppEmail,
						Origin = this.customerDetails.Origin,
						CreateDate = now,
						ExpectedEndDate = now.AddDays(7),
						RequestedAmount = lastLoanAmount.HasValue ? (int)lastLoanAmount.Value : (int?)null,
						Type = this.customerDetails.NumOfLoans == 0
							? OpportunityType.New.DescriptionAttr()
							: OpportunityType.Resell.DescriptionAttr(),
						Stage = OpportunityStage.s5.DescriptionAttr(),
						Name = this.customerDetails.FullName + cashRequestCount
					}
				).Execute();
			} // if
		} // CreateCashRequest

		private MarketplaceUpdateStatus UpdateMarketplaces() {
			if (!this.newCreditLineOption.UpdateData())
				return null;

			Log.Debug("Checking which marketplaces should be updated for customer {0}...", CustomerID);

			DateTime now = DateTime.UtcNow;

			var mpsToUpdate = new List<int>();
			var mpNamesToUpdate = new List<string>();

			DB.ForEachRowSafe(
				sr => {
					DateTime lastUpdateTime = sr["UpdatingEnd"];

					if ((now - lastUpdateTime).Days <= CurrentValues.Instance.UpdateOnReapplyLastDays)
						return;

					int mpID = sr["MpID"];

					mpsToUpdate.Add(mpID);

					if (sr["LongUpdateTime"])
						mpNamesToUpdate.Add(string.Format("{0} marketplace with id {1}", (string)sr["Name"], mpID));
				},
				"LoadMarketplacesLastUpdateTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", CustomerID)
			);

			if (mpsToUpdate.Count < 1) {
				Log.Debug("No marketplace should be updated for customer {0}.", CustomerID);
				return null;
			} // if

			if (mpNamesToUpdate.Count > 0) {
				Context.Description = string.Format(
					"This strategy can take long time (updating {0}).",
					string.Join(", ", mpNamesToUpdate)
				);
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

		private void NotifyAutoApproveSilentMode(
			decimal autoApprovedAmount,
			int repaymentPeriod,
			decimal interestRate,
			decimal setupFee,
			ATrail approvalTrail
		) {
			try {
				var message = string.Format(
					@"<h1><u>Silent auto approve for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b>Offer:</b></h2>
					<pre><h3>Amount: {1}</h3></pre><br>
					<pre><h3>Period: {2}</h3></pre><br>
					<pre><h3>Interest rate: {3}</h3></pre><br>
					<pre><h3>Setup fee: {4}</h3></pre><br>
					<h2><b>Decision flow:</b></h2>
					<pre><h3>{5}</h3></pre><br>
					<h2><b>Decision data:</b></h2>
					<pre><h3>{6}</h3></pre>", CustomerID,
					autoApprovedAmount.ToString("C0", Strategies.Library.Instance.Culture),
					repaymentPeriod,
					interestRate.ToString("P2", Strategies.Library.Instance.Culture),
					setupFee.ToString("P2", Strategies.Library.Instance.Culture),
					HttpUtility.HtmlEncode(approvalTrail.ToString()),
					HttpUtility.HtmlEncode(approvalTrail.InputData.Serialize())
				);

				new Mail().Send(
					CurrentValues.Instance.AutoApproveSilentToAddress,
					null,
					message,
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					"#SilentApprove for customer " + CustomerID
				);
			} catch (Exception e) {
				Log.Alert(e, "Failed sending alert mail - silent auto approval.");
			} // try
		} // NotifyAutoApproveSilentMode

		private void NLCashRequestAdd(long crID, DateTime now) {
			AddCashRequest nlAddCashRequest = new AddCashRequest(new NL_CashRequests {
				CashRequestOriginID = (int)(this.cashRequestOriginator ?? CashRequestOriginator.Other),
				CustomerID = CustomerID,
				OldCashRequestID = crID,
				RequestTime = now,
				UserID = UnderwriterID,
			});
			nlAddCashRequest.Context.CustomerID = CustomerID;
			nlAddCashRequest.Context.UserID = UnderwriterID;
			nlAddCashRequest.Execute();
			this.nlCashRequestID = nlAddCashRequest.CashRequestID;
		} // NLCashRequestAdd

		private readonly FinishWizardArgs finishWizardArgs;
		private readonly bool avoidAutomaticDecision;

		private readonly NewCreditLineOption newCreditLineOption;
		private readonly AutoDecisionResponse autoDecisionResponse;

		private readonly CustomerDetails customerDetails;

		private MedalResult medal;

		private readonly InternalCashRequestID cashRequestID;
		private long nlCashRequestID;

		private readonly StrategiesMailer mailer;

		/// <summary>
		/// Default: true. However when Main strategy is executed as a part of
		/// Finish Wizard strategy and customer is already approved/rejected
		/// then customer's status should not change.
		/// </summary>
		private readonly bool overrideApprovedRejected;

		private readonly CashRequestOriginator? cashRequestOriginator;

		private readonly string tag;
		private bool wasMismatch;
		private ABackdoorSimpleDetails backdoorSimpleDetails;
		private AutoRejectionOutput autoRejectionOutput;
	} // class MainStrategy
} // namespace
