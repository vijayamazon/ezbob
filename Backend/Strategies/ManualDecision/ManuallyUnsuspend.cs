namespace Ezbob.Backend.Strategies.ManualDecision {
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class ManuallyUnsuspend : AApplyManualDecisionBase {
		public ManuallyUnsuspend(DecisionToApply decision, AConnection db, ASafeLog log) : base(decision, db, log) {
		} // constructor
	} // class ManuallyUnsuspend
} // namespace
