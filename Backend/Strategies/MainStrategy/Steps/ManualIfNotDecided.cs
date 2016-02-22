namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal class ManualIfNotDecided : AOneExitStep {
		public ManualIfNotDecided(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription) {
			this.autoDecisionResponse = autoDecisionResponse;
		} // constructor

		protected override void ExecuteStep() {
			if (this.autoDecisionResponse.SystemDecision.HasValue)
				return;

			this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
			this.autoDecisionResponse.UserStatus = Status.Manual;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class ManualIfNotDecided
} // namespace
