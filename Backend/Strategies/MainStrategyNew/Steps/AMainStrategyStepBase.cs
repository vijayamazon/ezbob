namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using Ezbob.Logger;

	internal abstract class AMainStrategyStepBase {
		public abstract AMainStrategyStepBase Execute();

		public abstract bool IsTheLastOne { get; }

		protected AMainStrategyStepBase(string outerContextDescription) {
			if (string.IsNullOrWhiteSpace(outerContextDescription))
				throw new ArgumentNullException("outerContextDescription", "Context description not specified.");

			OuterContextDescription = outerContextDescription;
		} // constructor

		protected virtual string Name { get { return string.Format("'{0}'", GetType().Name); } }

		protected virtual ASafeLog Log { get { return Library.Instance.Log; } }

		protected string OuterContextDescription { get; private set; }
	} // class AMainStrategyStep
} // namespace
