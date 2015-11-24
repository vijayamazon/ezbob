namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	using DecisionActions = EZBob.DatabaseLib.Model.Database.Status;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallyApprove : AApplyManualDecisionBase {
		public ManuallyApprove(DecisionToApply decision, AConnection db, ASafeLog log) : base(decision, db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return
				base.HasValidParameters() &&
				(CashRequestManagerApprovedSum > 0) &&
				(CustomerManagerApprovedSum > 0) &&
				(CreditSum > 0) &&
				(NumApproves > 0);
		} // HasValidParameters

		public string Status { get { return DecisionActions.Approved.ToString(); } set { } }
		public DateTime DateApproved { get { return this.decision.Customer.DateApproved; } set { } }
		public string ApprovedReason { get { return this.decision.Customer.ApprovedReason; } set { } }
		public decimal CreditSum { get { return this.decision.Customer.CreditSum; } set { } }
		public decimal CustomerManagerApprovedSum { get { return this.decision.Customer.ManagerApprovedSum; } set { } }
		public int NumApproves { get { return this.decision.Customer.NumApproves; } set { } }
		public int IsLoanTypeSelectionAllowed { get { return this.decision.Customer.IsLoanTypeSelectionAllowed; } set { } }

		public int CashRequestManagerApprovedSum { get { return this.decision.CashRequest.ManagerApprovedSum; } set { } }
	} // class ManuallyApprove
} // namespace
