namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.Models;

	public class BankLikeLoanCalculator : ALoanCalculator {
		public BankLikeLoanCalculator(LoanCalculatorModel model) : base(model) {
		} // constructor

		public override string Name { get { return "Bank-like calculator"; } }

		/*/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <param name="periodCount">A number of periods to add.</param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		internal override DateTime AddPeriods(int periodCount) {
			return WorkingModel.IsMonthly
				? WorkingModel.LoanIssueTime.AddMonths(periodCount)
				: WorkingModel.LoanIssueTime.AddDays(periodCount * (int)WorkingModel.RepaymentIntervalType);
		} // AddPeriods*/

		protected override decimal CalculateDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			return monthlyInterestRate * 12.0m / 365.0m;
		} // CalculateDailyInterestRate
	} // class BankLikeCalculator
} // namespace
