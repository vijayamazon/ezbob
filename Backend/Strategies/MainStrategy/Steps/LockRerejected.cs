namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal class LockRerejected : AOneExitStep {
		public LockRerejected(
			string outerContextDescription,
			AutoDecisionResponse response
		) : base(outerContextDescription) {
			this.autoDecisionResponse = response;
		} // constructor

		protected override void ExecuteStep() {
			if (this.autoDecisionResponse.DecisionIsLocked)
				return;

			this.autoDecisionResponse.DecisionIsLocked = true;

			this.autoDecisionResponse.Decision = DecisionActions.ReReject;
			this.autoDecisionResponse.AutoRejectReason = "Auto Re-Reject";
			this.autoDecisionResponse.CreditResult = CreditResultStatus.Rejected;
			this.autoDecisionResponse.UserStatus = Status.Rejected;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Reject;
			this.autoDecisionResponse.DecisionName = "Re-rejection";
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class LockRerejected
} // namespace
