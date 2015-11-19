namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	using DecisionActions = EZBob.DatabaseLib.Model.Database.Status;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallyReject : AApplyManualDecisionBase {
		public ManuallyReject(DecisionToApply decision, AConnection db, ASafeLog log) : base(decision, db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return base.HasValidParameters() && (NumRejects > 0);
		} // HasValidParameters

		public string Status { get { return DecisionActions.Rejected.ToString(); } set { } }

		public DateTime DateRejected { get { return this.decision.Customer.DateRejected; } set { } }
		public string RejectedReason { get { return this.decision.Customer.RejectedReason; } set { } }
		public int NumRejects { get { return this.decision.Customer.NumRejects; } set { } }

		public List<int> RejectionReasons { get { return this.decision.CashRequest.RejectionReasons; } set { } }
	} // class ManuallyReject
} // namespace
