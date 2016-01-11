namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class BankLikeLoanCalculator : ALoanCalculator {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
		/// <exception cref="NotSupportedException">BankLikeLoanCalculator not suppoted yet</exception>
		public BankLikeLoanCalculator(NL_Model model, DateTime? calculationDate = null): base(model, calculationDate) {
			throw new NotSupportedException("BankLikeLoanCalculator not suppoted yet");
		} // constructor

		public override string Name { get { return "Bank-like calculator"; } }

		/// <exception cref="NotSupportedException">BankLikeLoanCalculator not suppoted yet</exception>
		public override decimal AverageDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null) {
			throw new NotSupportedException("BankLikeLoanCalculator not suppoted yet");
			return monthlyInterestRate * 12.0m / 365.0m;
		} // AverageDailyInterestRate

	} // class BankLikeCalculator
} // namespace
