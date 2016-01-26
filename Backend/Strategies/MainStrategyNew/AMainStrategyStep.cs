namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;

	internal abstract class AMainStrategyStep {
		public AMainStrategyStep Execute() {
			try {
				Log.Debug("Executing step {0} for {1}...", Name, ContextDescription);

				StepOutcome outcome = Run();

				Log.Debug(
					"Completed step {0} with result '{1}' for {2}.",
					Name,
					GetStepOutcomeName(outcome),
					ContextDescription
				);

				if (!NextSteps.ContainsKey(outcome)) {
					throw new Exception(string.Format(
						"No next step specified for outcome {0}.",
						GetStepOutcomeName(outcome)
					));
				} // if

				return NextSteps[outcome];
			} catch (Exception e) {
				Log.Alert(
					e,
					"Exception during step '{0}' while executing for {1}.",
					Name,
					ContextDescription
				);

				return NextSteps[StepOutcome.Exception];
			} // try
		} // Execute

		protected AMainStrategyStep(string contextDescription, AMainStrategyStep onException) {
			if (string.IsNullOrWhiteSpace(contextDescription))
				throw new ArgumentNullException("contextDescription", "Context description not specified.");

			if (onException == null)
				throw new ArgumentNullException("onException", "OnException handler cannot be NULL.");

			ContextDescription = contextDescription;

			NextSteps = new SortedDictionary<StepOutcome, AMainStrategyStep> {
				{ StepOutcome.Exception, onException },
			};
		} // constructor

		protected enum StepOutcome {
			Exception,
			One,
			Two,
			Three,
			Four,
		} // enum StepOutcome

		protected virtual string GetStepOutcomeName(StepOutcome outcome) {
			switch (outcome) {
			case StepOutcome.Exception:
				return "'exception'";

			case StepOutcome.One:
			case StepOutcome.Two:
			case StepOutcome.Three:
			case StepOutcome.Four:
				return string.Format("'not implemented ({0})'", outcome);

			default:
				throw new ArgumentOutOfRangeException("outcome");
			} // switch
		} // GetStepOutcomeName

		protected virtual string Name { get { return string.Format("'{0}'", GetType().Name); } }

		protected virtual ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		protected abstract StepOutcome Run();

		protected string ContextDescription { get; private set; }

		protected SortedDictionary<StepOutcome, AMainStrategyStep> NextSteps { get; private set; }
	} // class AMainStrategyStep
} // namespace
