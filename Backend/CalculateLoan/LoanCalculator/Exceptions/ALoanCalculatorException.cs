namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	using System;

	public abstract class ALoanCalculatorException : Exception {
		protected ALoanCalculatorException() {
		} // constructor

		protected ALoanCalculatorException(string msg)
			: base(msg) {
		} // constructor

		protected ALoanCalculatorException(string msg, Exception inner)
			: base(msg, inner) {
		} // constructor
	} // class ACalculateLoanException
} // namespace
