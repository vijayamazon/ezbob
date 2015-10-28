namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class LegacyLoanCalculator : ALoanCalculator {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		public LegacyLoanCalculator(NL_Model model) : base(model) {
		} // constructor

		public override string Name { get { return "Legacy calculator"; } }

		/// <exception cref="NoPeriodEndDateException">Condition. </exception>
		public override decimal AverageDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null) {

			if (periodEndDate == null)
				throw new NoPeriodEndDateException();

			DateTime d = periodEndDate.Value.AddMonths(-1);

			return monthlyInterestRate / DateTime.DaysInMonth(d.Year, d.Month);
		} // AverageDailyInterestRate

	} // class LegacyLoanCalculator
} // namespace
