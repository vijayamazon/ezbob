namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {

	public class LoanWriteOffStatusException : ALoanCalculatorException {

		public LoanWriteOffStatusException(long loanID)
			: base(string.Format("Loan {0} in status 'Write off'", loanID)) {
		}
	}
}