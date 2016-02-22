namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {

	public class LoanPaidOffStatusException : ALoanCalculatorException {
		public LoanPaidOffStatusException(long loanID): base(string.Format("Loan {0} is 'Paid off'", loanID)) {
		}
	}
}
