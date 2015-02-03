namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	using System;
	using Ezbob.Utils.Formula.Boolean;

	internal class CountAmount : IFittable {
		public CountAmount(Type opType, IFittable leftTerm, IFittable rightTerm) {
			this.leftTerm = leftTerm;
			this.rightTerm = rightTerm;

			if (typeof(ABinaryBoolOperator).IsAssignableFrom(opType))
				this.op = (ABinaryBoolOperator)Activator.CreateInstance(opType, leftTerm, rightTerm);
			else
				this.op = null;
		} // constructor

		public virtual void Init(
			int actualLoanCount,
			int? configuredLoanCount,
			decimal actualLoanAmount,
			decimal? configuredLoanLoanAmount
		) {
			if (this.leftTerm != null)
				this.leftTerm.Init(actualLoanCount, configuredLoanCount, actualLoanAmount, configuredLoanLoanAmount);

			if (this.rightTerm != null)
				this.rightTerm.Init(actualLoanCount, configuredLoanCount, actualLoanAmount, configuredLoanLoanAmount);
		} // Init

		public virtual bool Calculate() {
			return (this.op != null) && this.op.Calculate();
		} // Calculate

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0}({1}, {2})",
				this.op == null ? "NONE" : this.op.GetType().Name,
				this.leftTerm,
				this.rightTerm
			);
		} // ToString

		private readonly ABinaryBoolOperator op;
		private readonly IFittable leftTerm;
		private readonly IFittable rightTerm;
	} // class CountAmount
} // namespace
