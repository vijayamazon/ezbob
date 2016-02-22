namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NoPeriodEndDateException : ALoanCalculatorModelException {
		public NoPeriodEndDateException() : base("Legacy daily interest rate calculator requires period end date.") {
		} // constructor
	} // class NoPeriodEndDateException
} // namespace
