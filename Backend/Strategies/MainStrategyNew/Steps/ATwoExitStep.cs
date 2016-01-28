namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;

	internal abstract class ATwoExitStep : AMainStrategyStep {
		protected ATwoExitStep(
			string outerContextDescription,
			AMainStrategyStep firstExitStep,
			AMainStrategyStep secondExitStep
		) : base(outerContextDescription) {
			if (firstExitStep == null)
				throw new ArgumentNullException("firstExitStep", "First exit step cannot be NULL.");

			if (secondExitStep == null)
				throw new ArgumentNullException("secondExitStep", "Second exit step cannot be NULL.");

			FirstExit = firstExitStep;
			SecondExit = secondExitStep;
		} // constructor

		protected AMainStrategyStep FirstExit { get; private set; }

		protected AMainStrategyStep SecondExit { get; private set; }
	} // class ATwoExitStep
} // namespace
