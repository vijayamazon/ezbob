namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class LegacyLoanCalculator : ALoanCalculator {
		public LegacyLoanCalculator(NL_Model model) : base(model) {
		} // constructor

		public override string Name { get { return "Legacy calculator"; } }

		/// <exception cref="NoPeriodEndDateException">Condition. </exception>
		public override decimal CalculateDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null) {

			if (periodEndDate == null)
				throw new NoPeriodEndDateException();

			DateTime d = periodEndDate.Value.AddMonths(-1);

			return monthlyInterestRate / DateTime.DaysInMonth(d.Year, d.Month);
		} // CalculateDailyInterestRate

	} // class LegacyLoanCalculator
} // namespace
