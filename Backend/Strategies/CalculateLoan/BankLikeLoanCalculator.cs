namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate;

	public class BankLikeLoanCalculator : ALoanCalculator {
		public BankLikeLoanCalculator(LoanCalculatorModel model) : base(model, new BankLikeInterestRate()) {
		} // constructor

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <param name="periodCount">A number of periods to add.</param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		protected override DateTime AddPeriods(int periodCount) {
			return WorkingModel.IsMonthly
				? WorkingModel.LoanIssueTime.AddMonths(periodCount)
				: WorkingModel.LoanIssueTime.AddDays(periodCount * (int)WorkingModel.RepaymentIntervalType);
		} // AddPeriods
	} // class BankLikeCalculator
} // namespace
