namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionCustomerNotFound : Exception {

		public static string DefaultMessage = "No valid Customer ID";

		public NL_ExceptionCustomerNotFound(string message) : base(message) { }

		public NL_ExceptionCustomerNotFound(string message, Exception inner)
			: base(message, inner) { }
	}

}

