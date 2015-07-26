namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	using System;
	using Ezbob.Backend.CalculateLoan.Exceptions;

	public abstract class ALoanCalculatorException : ACalculateLoanException {
		protected ALoanCalculatorException() {
		} // constructor

		protected ALoanCalculatorException(string msg) : base(msg) {
		} // constructor

		protected ALoanCalculatorException(string msg, Exception inner) : base(msg, inner) {
		} // constructor
	} // class ALoanCalculatorException
} // namespace
