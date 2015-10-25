namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;

	public abstract class ALoanCalculatorModelException : ACalculateLoanException {
		protected ALoanCalculatorModelException() {
		} // constructor

		protected ALoanCalculatorModelException(string msg) : base(msg) {
		} // constructor

		protected ALoanCalculatorModelException(string msg, Exception inner) : base(msg, inner) {
		} // constructor
	} // class ALoanCalculatorModelException
} // namespace
