namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;

	internal abstract class AThreeExitStep : ATwoExitStep {
		protected AThreeExitStep(
			string outerContextDescription,
			AMainStrategyStep firstExitStep,
			AMainStrategyStep secondExitStep,
			AMainStrategyStep thirdExitStep
		) : base(outerContextDescription, firstExitStep, secondExitStep) {
			if (thirdExitStep == null)
				throw new ArgumentNullException("thirdExitStep", "Third exit step cannot be NULL.");

			ThirdExit = thirdExitStep;
		} // constructor

		protected AMainStrategyStep ThirdExit { get; private set; }
	} // class AThreeExitStep
} // namespace
