namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	using System;

	public abstract class ALoanCalculatorModelException : ALoanCalculatorException {
		protected ALoanCalculatorModelException() {
		} // constructor

		protected ALoanCalculatorModelException(string msg) : base(msg) {
		} // constructor

		protected ALoanCalculatorModelException(string msg, Exception inner) : base(msg, inner) {
		} // constructor
	} // class ALoanCalculatorModelException
} // namespace
