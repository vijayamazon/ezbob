namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	internal abstract class AOneExitStep : AMainStrategyStep {
		protected AOneExitStep(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		public override string Outcome { get { return "'completed'"; } }

		protected override StepResults Run() {
			ExecuteStep();
			return StepResults.Success;
		} // Run

		protected abstract void ExecuteStep();
	} // class AOneExitStep
} // namespace
