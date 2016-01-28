namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	internal class TheLastOne : AMainStrategyStepBase {
		public TheLastOne(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		public override AMainStrategyStepBase Execute() {
			Log.Msg("Main strategy is complete for {0}.", OuterContextDescription);
			return null;
		} // Execute
	} // class TheLastOne
} // namespace
