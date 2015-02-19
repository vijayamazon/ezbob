namespace Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate {
	using System;

	internal class BankLikeCalculator : ACalculator {
		public override decimal GetRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			return monthlyInterestRate * 12.0m / 365.0m;
		} // constructor
	} // class BankLikeCalculator
} // namespace
