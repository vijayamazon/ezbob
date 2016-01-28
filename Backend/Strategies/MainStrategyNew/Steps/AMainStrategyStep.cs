namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using Ezbob.Utils;

	internal abstract class AMainStrategyStep : AMainStrategyStepBase {
		public delegate void CollectOutputValueDelegate(string propertyName, object propertyValue);

		public event CollectOutputValueDelegate CollectOutputValue;

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

				CollectOutputValues();

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

		protected virtual bool ShouldCollectOutput { get { return true; } }

		private void CollectOutputValues() {
			if (!ShouldCollectOutput)
				return;

			if (CollectOutputValue == null)
				return;

			Log.Debug("Collecting output values of step {0} for {1}...", Name, OuterContextDescription);

			this.TraverseReadable((instance, pi) => {
				object[] oAttrList = pi.GetCustomAttributes(typeof(StepOutputAttribute), false);

				if (oAttrList.Length <= 0)
					return;

				Log.Debug("Collecting '{2}' property of step {0} for {1}...", Name, OuterContextDescription, pi.Name);
				CollectOutputValue(pi.Name, pi.GetValue(instance));
			});

			Log.Debug("Completed collecting output values of step {0} for {1}.", Name, OuterContextDescription);
		} // CollectOutputValues
	} // class AMainStrategyStep
} // namespace
