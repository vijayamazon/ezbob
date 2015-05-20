namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NegativeLoanAmountException : ANegativeDecimalException {
		public NegativeLoanAmountException(decimal loanAmount) : base("Loan amount is negative: {0}.", loanAmount) {
		} // constructor
	} // class NegativeLoanAmountException
} // namespace
