namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using System;
	using Ezbob.Logger;

	internal abstract class AMainStrategyStep {
		public AMainStrategyStep Execute() {
			try {
				Log.Debug("Executing step {0} for {1}...", Name, OuterContextDescription);

				AMainStrategyStep nextStep = Run();

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

				return this.onException;
			} // try
		} // Execute

		protected AMainStrategyStep(string outerContextDescription, AMainStrategyStep onException) {
			if (string.IsNullOrWhiteSpace(outerContextDescription))
				throw new ArgumentNullException("outerContextDescription", "Context description not specified.");

			if (onException == null)
				throw new ArgumentNullException("onException", "OnException handler cannot be NULL.");

			OuterContextDescription = outerContextDescription;

			this.onException = onException;
		} // constructor

		protected virtual string Name { get { return string.Format("'{0}'", GetType().Name); } }

		protected virtual ASafeLog Log { get { return Library.Instance.Log; } }

		protected abstract string Outcome { get; }

		protected abstract AMainStrategyStep Run();

		protected string OuterContextDescription { get; private set; }

		private readonly AMainStrategyStep onException;
	} // class AMainStrategyStep
} // namespace
