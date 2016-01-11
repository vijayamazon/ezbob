namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionRequiredDataNotFound : Exception {

		public static string Loan = "Expected input data (NL_Model initialized by: Loan) not found"; 
		public static string LastHistory = "History not found";
		public static string HistoryEventTime = "History EventTime not found";
		public static string OldLoan = "Loan.OldLoanID not found";
		public static string LoanRefNum = "Loan.Refnum not found";
		public static string OldID = "OldID not found";

		public NL_ExceptionRequiredDataNotFound() { }

		public NL_ExceptionRequiredDataNotFound(string message) : base(message) { }

		public NL_ExceptionRequiredDataNotFound(string message, Exception inner)
			: base(message, inner) { }
	}

}