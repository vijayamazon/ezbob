namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models;

	public class LegacyLoanCalculator : ALoanCalculator {
		public LegacyLoanCalculator(LoanCalculatorModel model) : base(model) {
		} // constructor

		public override string Name { get { return "Legacy calculator"; } }

		protected override decimal CalculateDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			if (periodEndDate == null)
				throw new NoPeriodEndDateException();

			DateTime d = periodEndDate.Value.AddMonths(-1);

			return monthlyInterestRate / DateTime.DaysInMonth(d.Year, d.Month);
		} // CalculateDailyInterestRate
	} // class LegacyLoanCalculator
} // namespace
