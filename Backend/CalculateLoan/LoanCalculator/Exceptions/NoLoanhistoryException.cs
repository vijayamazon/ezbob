namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {

	public class NoLoanHistoryException : ALoanCalculatorModelException {
		public NoLoanHistoryException()
			: base("History data not found")
		{
		} // constructor
	} // class NoLoanHistoryException
} // namespace
