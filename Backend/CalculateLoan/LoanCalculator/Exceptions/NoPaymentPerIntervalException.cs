namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class NoPaymentPerIntervalException : ALoanCalculatorModelException {
		public NoPaymentPerIntervalException()
			: base("No PaymentPerInterval found.") {
		} // constructor
	} // class NoScheduleException
} // namespace
