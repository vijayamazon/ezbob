﻿namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	internal class LockManual : AOneExitStep {
		public LockManual(
			string outerContextDescription,
			AMainStrategyStep nextStep,
			AutoDecisionResponse response
		) : base(outerContextDescription, nextStep) {
			this.autoDecisionResponse = response;
		} // constructor

		protected override void ExecuteStep() {
			if (this.autoDecisionResponse.DecisionIsLocked)
				return;

			this.autoDecisionResponse.DecisionIsLocked = true;

			this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
			this.autoDecisionResponse.UserStatus = Status.Manual;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class LockManual
} // namespace
