namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	public class NegativeRepaymentCountException : ALoanCalculatorModelException {
		public NegativeRepaymentCountException(int term) : base("Repayment count is negative: " + term + ".") {
		} // constructor
	} // class NegativeRepaymentCountException
} // namespace
