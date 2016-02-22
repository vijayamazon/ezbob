namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Backend.Strategies.MainStrategy.Steps;
	using Ezbob.Utils;

	internal class StepResult : IComparable<StepResult> {
		public StepResult(Type stepType, StepResults result) {
			if (stepType == null)
				throw new ArgumentNullException("stepType", "Step type not specified.");

			if (!TypeUtils.IsSubclassOf(stepType, typeof(AMainStrategyStepBase)))
				throw new ArgumentOutOfRangeException("stepType", "Step type is not inherited from base step type.");

			StepType = stepType.FullName;
			Result = result;
		} // constructor

		public string StepType { get; private set; }

		public StepResults Result { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format("{0} of {1}", Result, StepType);
		} // ToString

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
		public int CompareTo(StepResult other) {
			if (other == null)
				return 1;

			int n = string.Compare(StepType, other.StepType, StringComparison.InvariantCulture);

			if (n != 0)
				return n;

			return Result.CompareTo(other.Result);
		} // CompareTo
	} // class StepResult
} // namespace
