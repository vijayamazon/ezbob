namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionLoanExists : Exception {

		public static string DefaultMessage = "NL Loan exists.";

		public NL_ExceptionLoanExists() { }

		public NL_ExceptionLoanExists(string message)
			: base(message) { }

		public NL_ExceptionLoanExists(string message, Exception inner)
			: base(message, inner) { }
	}
}