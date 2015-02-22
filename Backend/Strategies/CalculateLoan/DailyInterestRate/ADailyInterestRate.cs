namespace Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate {
	using System;

	public abstract class ADailyInterestRate {
		public abstract decimal GetRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		);

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return this.GetType().Name;
		} // ToString

		protected virtual bool InitHasRun { get; set; }
	} // class ADailyInterestRate
} // namespace
