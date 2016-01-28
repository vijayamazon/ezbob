namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;

	internal abstract class AMainStrategyStep : AMainStrategyStepBase {
		public sealed override bool IsTheLastOne { get { return false; } }

		public override AMainStrategyStepBase Execute() {
			try {
				Log.Debug("Executing step {0} for {1}...", Name, OuterContextDescription);

				AMainStrategyStepBase nextStep = Run();

				Log.Debug(
					"Completed step {0} with result '{1}' for {2}.",
					Name,
					Outcome,
					OuterContextDescription
				);

				if (nextStep == null)
					throw new Exception(string.Format("No next step specified for outcome {0}.", Outcome));

				return nextStep;
			} catch (Exception e) {
				Log.Alert(
					e,
					"Exception during step '{0}' while executing for {1}.",
					Name,
					OuterContextDescription
				);

				return new TheLastOne(OuterContextDescription);
			} // try
		} // Execute

		protected AMainStrategyStep(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		protected abstract string Outcome { get; }

		protected abstract AMainStrategyStepBase Run();
	} // class AMainStrategyStep
} // namespace
