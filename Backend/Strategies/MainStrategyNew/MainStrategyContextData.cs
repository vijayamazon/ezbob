namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using System;
	using System.Globalization;
	using AutomationCalculator.Common;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using CashRequestOriginatorType = EZBob.DatabaseLib.Model.Database.CashRequestOriginator;

	internal class MainStrategyContextData {
		public MainStrategyContextData(MainStrategyArguments args) {
			this.arguments = args;

			AutoRejectionOutput = null;

			OverrideApprovedRejected = true;
			CashRequestID = new InternalCashRequestID(this.arguments.CashRequestID);
			CashRequestOriginator = this.arguments.CashRequestOriginator;

			if (FinishWizardArgs != null) {
				CashRequestOriginator = FinishWizardArgs.CashRequestOriginator;
				FinishWizardArgs.DoMain = false;
				OverrideApprovedRejected = FinishWizardArgs.CashRequestOriginator != CashRequestOriginatorType.Approved;
			} // if

			WasMismatch = false;

			AutoDecisionResponse = new AutoDecisionResponse();

			Tag = string.Format(
				"#MainStrategy_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString().ToUpperInvariant()
			);

			CustomerDetails = new Strategies.MainStrategy.CustomerDetails(this.arguments.CustomerID);
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

		public Strategies.MainStrategy.CustomerDetails CustomerDetails { get; private set; }

		public MedalResult Medal { get; set; }

		public InternalCashRequestID CashRequestID { get; private set; }
		public long NLCashRequestID { get; set; }

		public bool WasMismatch { get; set; }

		public int CompanyID { get; set; }
		public MonthlyRepaymentData MonthlyRepayment { get; set; }

		public bool EnableAutomaticApproval { get { return CurrentValues.Instance.EnableAutomaticApproval; } }
		public bool EnableAutomaticReApproval { get { return CurrentValues.Instance.EnableAutomaticReApproval; } }
		public bool EnableAutomaticRejection { get { return CurrentValues.Instance.EnableAutomaticRejection; } }
		public bool EnableAutomaticReRejection { get { return CurrentValues.Instance.EnableAutomaticReRejection; } }
		public int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		public int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }
		public bool BackdoorEnabled { get { return CurrentValues.Instance.BackdoorSimpleAutoDecisionEnabled; } }
		public int MarketplaceUpdateValidityDays { get { return CurrentValues.Instance.UpdateOnReapplyLastDays; } }
		public bool LogicalGlueEnabled { get { return CurrentValues.Instance.LogicalGlueEnabled; } }

		private readonly MainStrategyArguments arguments;
	} // class MainStrategyContextData
} // namespace
