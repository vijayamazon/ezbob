namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal abstract class ABase : IFittable {
		public abstract void Init(
			int actualLoanCount,
			int? configuredLoanCount,
			decimal actualLoanAmount,
			decimal? configuredLoanLoanAmount
		);

		public abstract bool Calculate();

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return this.GetType().Name;
		} // ToString
	} // class ABase
} // namespace
