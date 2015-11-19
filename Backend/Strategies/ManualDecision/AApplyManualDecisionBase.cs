namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal abstract class AApplyManualDecisionBase : AStoredProcedure {
		protected AApplyManualDecisionBase(DecisionToApply decision, AConnection db, ASafeLog log) : base(db, log) {
			this.decision = decision;
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID > 0) && (CashRequestID > 0);
		} // HasValidParameters

		public int CustomerID { get { return this.decision.Customer.ID; } set { } }
		public string CreditResult { get { return this.decision.Customer.CreditResult; } set { } }
		public string UnderwriterName { get { return this.decision.Customer.UnderwriterName; } set { } }
		public bool? IsWaitingForSignature { get { return this.decision.Customer.IsWaitingForSignature; } set { } }

		public long CashRequestID { get { return this.decision.CashRequest.ID; } set { } }
		public byte[] CashRequestRowVersion { get { return this.decision.CashRequest.RowVersion; } set { } }
		public int UnderwriterID { get { return this.decision.CashRequest.UnderwriterID; } set { } }
		public DateTime UnderwriterDecisionDate { get { return this.decision.CashRequest.UnderwriterDecisionDate; } set { } }
		public string UnderwriterDecision { get { return this.decision.CashRequest.UnderwriterDecision; } set { } }
		public string UnderwriterComment { get { return this.decision.CashRequest.UnderwriterComment; } set { } }

		protected readonly DecisionToApply decision;
	} // class ManuallyUnsuspend
} // namespace
