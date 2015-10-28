namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	using System;

	public abstract class ACalculateLoanException : Exception {
		protected ACalculateLoanException() {
		} // constructor

		protected ACalculateLoanException(string msg) : base(msg) {
		} // constructor

		protected ACalculateLoanException(string msg, Exception inner) : base(msg, inner) {
		} // constructor
	} // class ACalculateLoanException
} // namespace

