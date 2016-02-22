namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionLoanNotFound : Exception {

		public static string DefaultMessage = "No valid LoanID";

		public NL_ExceptionLoanNotFound(string message) : base(message) { }

		public NL_ExceptionLoanNotFound(string message, Exception inner)
			: base(message, inner) { }
	}

}

