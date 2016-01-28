namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;

	internal abstract class AOneExitStep : AMainStrategyStep {
		protected AOneExitStep(string outerContextDescription, AMainStrategyStep nextStep) : base(outerContextDescription) {
			if (nextStep == null)
				throw new ArgumentNullException("nextStep", "Next step cannot be NULL.");

			NextStep = nextStep;
		} // constructor

		protected override string Outcome { get { return "'completed'"; } }

		protected AMainStrategyStep NextStep { get; private set; }

		protected override AMainStrategyStepBase Run() {
			ExecuteStep();
			return NextStep;
		} // Run

		protected abstract void ExecuteStep();
	} // class AOneExitStep
} // namespace
