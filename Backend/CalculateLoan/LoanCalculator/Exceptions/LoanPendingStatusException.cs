namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {

	public class LoanPendingStatusException : ALoanCalculatorException {

		public LoanPendingStatusException(long loanID)
			: base(string.Format("Loan {0} is in status 'Pending'", loanID)) {
		}
	}
}