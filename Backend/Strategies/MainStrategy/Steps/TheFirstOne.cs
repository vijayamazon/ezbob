namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal class TheFirstOne : AOneExitStep {
		public TheFirstOne(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		protected override void ExecuteStep() {
			Log.Msg("Main strategy started for {0}.", OuterContextDescription);
		} // ExecuteStep
	} // class TheFirstOne
} // namespace
