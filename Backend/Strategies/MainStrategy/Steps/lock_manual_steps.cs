namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;

	internal class PreventAutoDecision : ALockManual {
		public PreventAutoDecision(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor
	} // class PreventAutoDecision

	internal class LockManualAfterRereject : ALockManual {
		public LockManualAfterRereject(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor
	} // class LockManualAfterRereject

	internal class LockManualAfterReject : ALockManual {
		public LockManualAfterReject(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor
	} // class LockManualAfterReject

	internal class LockManualAfterOffer : ALockManual {
		public LockManualAfterOffer(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor
	} // class LockManualAfterOffer

	internal class LockManualAfterReapproval : ALockManual {
		public LockManualAfterReapproval(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor
	} // class LockManualAfterReapproval

	internal class LockManualAfterApproval : ALockManual {
		public LockManualAfterApproval(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor
	} // class LockManualAfterApproval

	internal class ForceManual : ALockManual {
		public ForceManual(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription, autoDecisionResponse) {
		} // constructor

		protected override void ExecuteStep() {
			LockManual();
		} // ExecuteStep
	} // class ForceManual
} // namespace
