namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NoLoanHistoryException : InvalidInitialDataException {
		public NoLoanHistoryException()
			: base("History data not found")
		{
		} // constructor
	} // class NoLoanHistoryException
} // namespace
