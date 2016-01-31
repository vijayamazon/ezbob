namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal class LockRejected : AOneExitStep {
		public LockRejected(
			string outerContextDescription,
			AutoDecisionResponse response
		) : base(outerContextDescription) {
			this.autoDecisionResponse = response;
		} // constructor

		protected override void ExecuteStep() {
			if (this.autoDecisionResponse.DecisionIsLocked)
				return;

			this.autoDecisionResponse.DecisionIsLocked = true;

			this.autoDecisionResponse.CreditResult = CreditResultStatus.Rejected;
			this.autoDecisionResponse.UserStatus = Status.Rejected;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Reject;
			this.autoDecisionResponse.DecisionName = "Rejection";
			this.autoDecisionResponse.Decision = DecisionActions.Reject;
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class LockRejected
} // namespace
