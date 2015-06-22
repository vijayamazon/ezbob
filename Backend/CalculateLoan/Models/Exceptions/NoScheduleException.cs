namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NoScheduleException : ALoanCalculatorModelException {
		public NoScheduleException() : base("No loan schedule found.") {
		} // constructor
	} // class NoScheduleException
} // namespace
