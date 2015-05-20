namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NullLoanCalculatorModelException : ALoanCalculatorException {
		public NullLoanCalculatorModelException() : base("No data for loan calculation.") {
		} // constructor
	} // class NullLoanCalculatorModelException
} // namespace
