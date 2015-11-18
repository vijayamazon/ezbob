namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	using DecisionActions = EZBob.DatabaseLib.Model.Database.Status;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallyReject : AStoredProcedure {
		public ManuallyReject(DecisionToApply decision, AConnection db, ASafeLog log) : base(db, log) {
			this.decision = decision;
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID > 0) && (CashRequestID > 0);
		} // HasValidParameters

		public int CustomerID { get { return this.decision.Customer.ID; } set { } }
		public string CreditResult { get { return this.decision.Customer.CreditResult; } set { } }
		public string Status { get { return DecisionActions.Rejected.ToString(); } set { } }
		public string UnderwriterName { get { return this.decision.Customer.UnderwriterName; } set { } }
		public DateTime DateRejected { get { return this.decision.Customer.DateRejected; } set { } }
		public string RejectedReason { get { return this.decision.Customer.RejectedReason; } set { } }
		public int NumRejects { get { return this.decision.Customer.NumRejects; } set { } }

		public long CashRequestID { get { return this.decision.CashRequest.ID; } set { } }
		public int UnderwriterID { get { return this.decision.CashRequest.UnderwriterID; } set { } }
		public DateTime UnderwriterDecisionDate { get { return this.decision.CashRequest.UnderwriterDecisionDate; } set { } }
		public string UnderwriterDecision { get { return this.decision.CashRequest.UnderwriterDecision; } set { } }
		public string UnderwriterComment { get { return this.decision.CashRequest.UnderwriterComment; } set { } }

		public List<int> RejectionReasons { get { return this.decision.CashRequest.RejectionReasons; } set { } }

		private readonly DecisionToApply decision;
	} // class ManuallyReject
} // namespace
