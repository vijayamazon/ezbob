namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NoInitialDataException : ALoanCalculatorModelException {
		public NoInitialDataException(string msg = null)
			: base(string.Format("Initial data was not specified. {0}", msg)) {
		} // constructor
	} // class NoInitialDataException
} // namespace
