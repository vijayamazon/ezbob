namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class AMainStrategyStepBase : IComparable<AMainStrategyStepBase> {
		public abstract StepResults Execute();

		public abstract string Outcome { get; }

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared.
		/// The return value has the following meanings:
		/// Less than zero: this object is less than the <paramref name="other"/> parameter.
		/// Zero: this object is equal to <paramref name="other"/>.
		/// Greater than zero: this object is greater than <paramref name="other"/>. 
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public int CompareTo(AMainStrategyStepBase other) {
			return other == null
				? 1
				: string.Compare(GetType().FullName, other.GetType().FullName, StringComparison.InvariantCulture);
		} // CompareTo

		public virtual string Name { get { return string.Format("'{0}'", GetType().Name); } }

		protected AMainStrategyStepBase(string outerContextDescription) {
			if (string.IsNullOrWhiteSpace(outerContextDescription))
				throw new ArgumentNullException("outerContextDescription", "Context description not specified.");

			OuterContextDescription = outerContextDescription;
		} // constructor

		protected virtual ASafeLog Log { get { return Library.Instance.Log; } }

		protected virtual AConnection DB { get { return Library.Instance.DB; } }

		protected string OuterContextDescription { get; private set; }
	} // class AMainStrategyStep
} // namespace
