namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;

	public abstract class ALoanCalculatorException : ACalculateLoanException {
		protected ALoanCalculatorException() {
		} // constructor

		protected ALoanCalculatorException(string msg) : base(msg) {
		} // constructor

		protected ALoanCalculatorException(string msg, Exception inner) : base(msg, inner) {
		} // constructor
	} // class ALoanCalculatorException
} // namespace
