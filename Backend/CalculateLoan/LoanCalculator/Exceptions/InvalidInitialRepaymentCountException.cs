namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialRepaymentCountException : ALoanCalculatorModelException {
		public InvalidInitialRepaymentCountException(int repaymentCount): base(string.Format("Invalid initial repayment count of {0} was specified.", repaymentCount)) {
		} // constructor
	}
} // class InvalidInitialRepaymentCountException