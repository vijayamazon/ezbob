namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal class TheLastOne : AMainStrategyStepBase {
		public TheLastOne(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		public override string Outcome { get { return "'completed'"; } }

		public override StepResults Execute() {
			Log.Msg("Main strategy is complete for {0}.", OuterContextDescription);
			return StepResults.NormalShutdown;
		} // Execute
	} // class TheLastOne
} // namespace
