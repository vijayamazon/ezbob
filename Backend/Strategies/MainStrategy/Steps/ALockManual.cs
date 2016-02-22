namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal abstract class ALockManual : AOneExitStep {
		protected ALockManual(
			string outerContextDescription,
			AutoDecisionResponse response
		) : base(outerContextDescription) {
			this.autoDecisionResponse = response;
		} // constructor

		protected override void ExecuteStep() {
			if (this.autoDecisionResponse.DecisionIsLocked)
				return;

			LockManual();
		} // ExecuteStep

		protected virtual void LockManual() {
			this.autoDecisionResponse.DecisionIsLocked = true;

			this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
			this.autoDecisionResponse.UserStatus = Status.Manual;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
		} // LockManual

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class ALockManual
} // namespace
