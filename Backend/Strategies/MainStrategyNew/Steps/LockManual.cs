namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal class LockManual : AOneExitStep {
		public LockManual(
			string outerContextDescription,
			AMainStrategyStep nextStep,
			AutoDecisionResponse response
		) : base(outerContextDescription, nextStep) {
			this.response = response;
		} // constructor

		protected override void ExecuteStep() {
			if (this.response.DecisionIsLocked)
				return;

			this.response.DecisionIsLocked = true;
			this.response.CreditResult = CreditResultStatus.WaitingForDecision;
			this.response.UserStatus = Status.Manual;
			this.response.SystemDecision = SystemDecision.Manual;
		} // ExecuteStep

		private readonly AutoDecisionResponse response;
	} // class LockManual
} // namespace
