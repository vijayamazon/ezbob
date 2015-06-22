namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NegativeInterestOnlyRepaymentCountException : ALoanCalculatorModelException {
		public NegativeInterestOnlyRepaymentCountException(int term)
			: base("Interest only repayment count is negative: " + term + ".") {
		} // constructor
	} // class NegativeInterestOnlyRepaymentCountException
} // namespace
