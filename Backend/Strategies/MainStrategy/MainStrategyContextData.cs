namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Globalization;
	using AutomationCalculator.Common;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using EZBob.DatabaseLib.Model.Database;
	using CashRequestOriginatorType = EZBob.DatabaseLib.Model.Database.CashRequestOriginator;

	internal class MainStrategyContextData {
		public MainStrategyContextData(MainStrategyArguments args) {
			this.arguments = args;

			IsSilentlyApproved = false;
			LoanOfferEmailSendingBannedNew = false;

			AutoRejectionOutput = null;
			AutoReapprovalOutput = null;
			AutoApprovalTrailUniqueID = null;

			OverrideApprovedRejected = true;
			CashRequestID = this.arguments.CashRequestID ?? 0;
			NLCashRequestID = 0;
			CashRequestOriginator = this.arguments.CashRequestOriginator;

			if (FinishWizardArgs != null) {
				CashRequestOriginator = FinishWizardArgs.CashRequestOriginator;
				FinishWizardArgs.DoMain = false;
				OverrideApprovedRejected = FinishWizardArgs.CashRequestOriginator != CashRequestOriginatorType.Approved;
			} // if

			WasMismatch = false;

			AutoDecisionResponse = new AutoDecisionResponse(this.arguments.CustomerID);

			Tag = string.Format(
				"#MainStrategy_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString().ToUpperInvariant()
			);

			CustomerDetails = new Helpers.CustomerDetails(this.arguments.CustomerID);

			HasCashRequest = false;
			CashRequestWasWritten = false;
			ShuttingDownUbnormally = false;
			DelayReason = string.Empty;
			CurrentStepName = "not started";

			WriteDecisionOutput = null;

			BackdoorLogicApplied = false;
			BackdoorInvestorID = null;
		} // constructor

		public string Description {
			get {
				return string.Format(
					"customer {0} by UW {1} ({2} from {3} with cash request {4} / {5}, tag {6})",
					CustomerID,
					UnderwriterID,
					NewCreditLineOption,
					CashRequestOriginator.HasValue ? CashRequestOriginator.ToString() : "'UNKNOWN ORIGINATOR'",
					CashRequestID,
					NLCashRequestID,
					Tag
				);
			} // get
		} // Description

		public AutoRejectionOutput AutoRejectionOutput { get; set; }
		public AutoReapprovalOutput AutoReapprovalOutput { get; set; }
		public Guid? AutoApprovalTrailUniqueID { get; set; }

		public bool IsSilentlyApproved { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }

		public NewCreditLineOption NewCreditLineOption {
			get { return this.arguments.NewCreditLine; }
		} // NewCreditLineOption

		public bool UpdateData {
			get { return NewCreditLineOption.UpdateData(); }
		} // UpdateData

		public bool AvoidAutoDecision {
			get { return NewCreditLineOption.AvoidAutoDecision() || (this.arguments.AvoidAutoDecision == 1); }
		} // UpdateData

		public AutoDecisionResponse AutoDecisionResponse { get; private set; }

		public int ProposedAmount {
			get { return AutoDecisionResponse.ProposedAmount; }
			set { AutoDecisionResponse.ProposedAmount = value; }
		} // ProposedAmount

		public int ApprovedAmount {
			get { return AutoDecisionResponse.ApprovedAmount; }
			set { AutoDecisionResponse.ApprovedAmount = value; }
		} // ApprovedAmount

		public int UnderwriterID { get { return this.arguments.UnderwriterID; } }

		public int CustomerID {
			get { return this.arguments.CustomerID; }
		} // CustomerID

		public FinishWizardArgs FinishWizardArgs {
			get { return this.arguments.FinishWizardArgs; }
		} // FinishWizardArgs

		/// <summary>
		/// Default: true. However when Main strategy is executed as a part of
		/// Finish Wizard strategy and customer is already approved/rejected
		/// then customer's status should not change.
		/// </summary>
		public bool OverrideApprovedRejected { get; private set; }

		public CashRequestOriginatorType? CashRequestOriginator { get; private set; }

		public string Tag { get; private set; }

		public Helpers.CustomerDetails CustomerDetails { get; private set; }

		public MedalResult Medal { get; set; }

		public long CashRequestID { get; private set; }
		public long NLCashRequestID { get; set; }

		public bool WasMismatch { get; set; }

		public bool BackdoorLogicApplied { get; set; }
		public int? BackdoorInvestorID { get; set; }

		public int CompanyID { get; set; }
		public TypeOfBusiness TypeOfBusiness { get; set; }
		public MonthlyRepaymentData MonthlyRepayment { get; set; }

		public OfferResult OfferResult { get; set; }
		public int LoanSourceID { get; set; }

		public string CurrentStepName { get; set; }
		public string DelayReason { get; set; }
		public bool ShuttingDownUbnormally { get; set; }
		public bool HasCashRequest { get; set; }
		public bool CashRequestWasWritten { get; set; }
		public WriteDecisionOutput WriteDecisionOutput { get; set; }

		public bool EnableAutomaticApproval { get { return CurrentValues.Instance.EnableAutomaticApproval; } }
		public bool EnableAutomaticReApproval { get { return CurrentValues.Instance.EnableAutomaticReApproval; } }
		public bool EnableAutomaticRejection { get { return CurrentValues.Instance.EnableAutomaticRejection; } }
		public bool EnableAutomaticReRejection { get { return CurrentValues.Instance.EnableAutomaticReRejection; } }
		public int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		public int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }
		public bool BackdoorEnabled { get { return CurrentValues.Instance.BackdoorSimpleAutoDecisionEnabled; } }
		public int MarketplaceUpdateValidityDays { get { return CurrentValues.Instance.UpdateOnReapplyLastDays; } }
		public bool LogicalGlueEnabled { get { return CurrentValues.Instance.LogicalGlueEnabled; } }
		public int MaxLoanAmount { get { return CurrentValues.Instance.AutoApproveMaxAmount; } }
		public int MinLoanAmount { get { return CurrentValues.Instance.MinLoan; } }
		public bool AutoApproveIsSilent { get { return CurrentValues.Instance.AutoApproveIsSilent; } }
		public int OfferValidForHours { get { return CurrentValues.Instance.OfferValidForHours; } }
		public string SilentEmailRecipient { get { return CurrentValues.Instance.AutoApproveSilentToAddress; } }
		public string SilentEmailSenderAddress { get { return CurrentValues.Instance.MailSenderEmail; } }
		public string SilentEmailSenderName { get { return CurrentValues.Instance.MailSenderName; } }
		public int SmallLoanScenarioLimit { get { return CurrentValues.Instance.SmallLoanScenarioLimit; } }
		public bool AspireToMinSetupFee { get { return CurrentValues.Instance.AspireToMinSetupFee; } }

		private readonly MainStrategyArguments arguments;
	} // class MainStrategyContextData
} // namespace
