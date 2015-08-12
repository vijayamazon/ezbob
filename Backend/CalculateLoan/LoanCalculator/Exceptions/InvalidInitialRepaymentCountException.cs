namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialInterestOnlyRepaymentCountException : InvalidInitialDataException {
		public InvalidInitialInterestOnlyRepaymentCountException(int interestOnlyRepaymentCount, int repaymentCount)
			: base(
				"Invalid initial interest only repayment count of {0} was specified (expected between 0 and {1}).",
				interestOnlyRepaymentCount,
				repaymentCount
			)
		{
		} // constructor
	} // class InvalidInitialInterestOnlyRepaymentCountException
} // namespace
