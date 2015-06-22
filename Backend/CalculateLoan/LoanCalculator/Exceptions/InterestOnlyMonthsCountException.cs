namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InterestOnlyMonthsCountException : ALoanCalculatorException {
		public InterestOnlyMonthsCountException(int interestOnlyRepaymentCount, int loanRepaymentCount)
			: base(string.Format(
				"Interest only repayment count ({0}) is not less than loan repayment count ({1}).",
				interestOnlyRepaymentCount,
				loanRepaymentCount
			)) {
		} // constructor
	} // class InterestOnlyMonthsCountException
} // namespace
