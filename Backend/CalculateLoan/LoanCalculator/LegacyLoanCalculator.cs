namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models;

	public class LegacyLoanCalculator : ALoanCalculator {
		public LegacyLoanCalculator(LoanCalculatorModel model) : base(model) {
		} // constructor

		public override string Name { get { return "Legacy calculator"; } }

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is always 1 month.
		/// </summary>
		/// <param name="periodCount">A number of months to add.</param>
		/// <returns>Date after requested number of months have been added to loan issue date.</returns>
		internal override DateTime AddPeriods(int periodCount) {
			return WorkingModel.LoanIssueTime.AddMonths(periodCount);
		} // AddPeriods

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
