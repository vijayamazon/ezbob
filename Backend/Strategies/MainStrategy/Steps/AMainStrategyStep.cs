namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Utils;

	internal abstract class AMainStrategyStep : AMainStrategyStepBase {
		public delegate void CollectOutputValueDelegate(string propertyName, object propertyValue);

		public event CollectOutputValueDelegate CollectOutputValue;

		public override StepResults Execute() {
			try {
				Log.Debug("Start of step {0} for {1}...", Name, OuterContextDescription);

				StepResults nextStep = Run();

				Log.Debug(
					"End of step {0} with result {1} for {2}.",
					Name,
					Outcome,
					OuterContextDescription
				);

				CollectOutputValues();

				return nextStep;
			} catch (Exception e) {
				Log.Alert(
					e,
					"Exception during step {0} while executing for {1}.",
					Name,
					OuterContextDescription
				);

				return StepResults.AbnormalShutdown;
			} // try
		} // Execute

		protected AMainStrategyStep(string outerContextDescription) : base(outerContextDescription) {
		} // constructor

		protected abstract StepResults Run();

		protected virtual bool ShouldCollectOutput { get { return true; } }

		private void CollectOutputValues() {
			if (!ShouldCollectOutput)
				return;

			if (CollectOutputValue == null)
				return;

			Log.Debug("Start of collecting output values of step {0} for {1}...", Name, OuterContextDescription);

			this.TraverseReadable((instance, pi) => {
				object[] oAttrList = pi.GetCustomAttributes(typeof(StepOutputAttribute), false);

				if (oAttrList.Length <= 0)
					return;

				Log.Debug("Collecting '{2}' property of step {0} for {1}...", Name, OuterContextDescription, pi.Name);
				CollectOutputValue(pi.Name, pi.GetValue(instance));
			});

			Log.Debug("End of collecting output values of step {0} for {1}.", Name, OuterContextDescription);
		} // CollectOutputValues
	} // class AMainStrategyStep
} // namespace
