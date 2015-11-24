namespace Ezbob.Backend.Strategies.ManualDecision {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallyEscalate : AApplyManualDecisionBase {
		public ManuallyEscalate(DecisionToApply decision, AConnection db, ASafeLog log) : base(decision, db, log) {
		} // constructor

		public DateTime DateEscalated { get { return this.decision.Customer.DateEscalated; } set { } }
		public string EscalationReason { get { return this.decision.Customer.EscalationReason; } set { } }
	} // class ManuallyEscalate
} // namespace
