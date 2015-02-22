namespace Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate {
	using System;

	internal class LegacyInterestRate : ADailyInterestRate {
		public override decimal GetRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			if (periodEndDate == null) {
				throw new ArgumentNullException(
					"periodEndDate",
					"Legacy daily interest rate calculator requires period end date."
				);
			} // if

			DateTime d = periodEndDate.Value.AddMonths(-1);

			return monthlyInterestRate / DateTime.DaysInMonth(d.Year, d.Month);
		} // GetRate
	} // class LegacyInterestRate
} // namespace
