namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NegativeRepaymentCountException : ALoanCalculatorModelException {
		public NegativeRepaymentCountException(int term) : base("Repayment count is negative: " + term + ".") {
		} // constructor
	} // class NegativeRepaymentCountException
} // namespace
