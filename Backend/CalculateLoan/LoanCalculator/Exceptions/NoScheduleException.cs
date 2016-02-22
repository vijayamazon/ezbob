namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NoScheduleException : ALoanCalculatorModelException {
		public NoScheduleException() : base("No loan schedule found.") {
		} // constructor
	} // class NoScheduleException
} // namespace
