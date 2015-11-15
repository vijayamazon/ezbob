namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class LegacyLoanCalculator : ALoanCalculator {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
		public LegacyLoanCalculator(NL_Model model, DateTime? calculationDate = null)
			: base(model, calculationDate) {
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