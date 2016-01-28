﻿namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	internal class TheFirstOne : AOneExitStep {
		public TheFirstOne(
			string outerContextDescription,
			AMainStrategyStep nextStep
		) : base(outerContextDescription, nextStep) {
		} // constructor

		protected override void ExecuteStep() {
			Log.Msg("Main strategy started for {0}.", OuterContextDescription);
		} // ExecuteStep
	} // class TheFirstOne
} // namespace
