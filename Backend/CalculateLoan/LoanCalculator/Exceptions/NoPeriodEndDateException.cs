namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	public class NoPeriodEndDateException : ALoanCalculatorException {
		public NoPeriodEndDateException() : base("Legacy daily interest rate calculator requires period end date.") {
		} // constructor
	} // class NoPeriodEndDateException
} // namespace
