namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NoInitialDataException : InvalidInitialDataException {
		public NoInitialDataException() : base("Initial data was not specified.") {
		} // constructor
	} // class NoInitialDataException
} // namespace
