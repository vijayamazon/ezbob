namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal class SetPendingInvestor : AOneExitStep {
		public SetPendingInvestor(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription) {
			this.autoDecisionResponse = autoDecisionResponse;
		} // constructor

		protected override void ExecuteStep() {
			Log.Info("Investor was not found for {0}, switching to manual.", OuterContextDescription);

			this.autoDecisionResponse.CreditResult = CreditResultStatus.PendingInvestor;
			this.autoDecisionResponse.UserStatus = Status.Manual;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class SetPendingInvestor
} // namespace
