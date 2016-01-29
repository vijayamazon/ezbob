namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	internal abstract class AOneExitStep : AMainStrategyStep {
		protected AOneExitStep(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		protected override string Outcome { get { return "'completed'"; } }

		protected override StepResults Run() {
			ExecuteStep();
			return StepResults.Completed;
		} // Run

		protected abstract void ExecuteStep();
	} // class AOneExitStep
} // namespace
