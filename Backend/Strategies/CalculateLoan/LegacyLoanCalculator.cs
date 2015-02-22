namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate;

	public class LegacyLoanCalculator : ALoanCalculator {
		public LegacyLoanCalculator(LoanCalculatorModel model) : base(model, new LegacyInterestRate()) {
		} // constructor

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is always 1 month.
		/// </summary>
		/// <param name="periodCount">A number of months to add.</param>
		/// <returns>Date after requested number of months have been added to loan issue date.</returns>
		protected override DateTime AddPeriods(int periodCount) {
			return WorkingModel.LoanIssueTime.AddMonths(periodCount);
		} // AddPeriods
	} // class LegacyLoanCalculator
} // namespace
