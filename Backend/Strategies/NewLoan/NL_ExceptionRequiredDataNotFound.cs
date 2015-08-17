namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class NL_ExceptionRequiredDataNotFound : Exception {

		public static string LastHistory = "History not found";
		public static string HistoryEventTime = "Histories.NL_LoanHistory.EventTime not found";
		public static string Loan = "Expected input data not found (NL_Model initialized by: Loan.OldLoanID, Loan.Refnum) not found";
		public static string OldLoan = "Loan.OldLoanID not found";
		public static string LoanRefNum = "Loan.Refnum not found";

		public NL_ExceptionRequiredDataNotFound(){}

        public NL_ExceptionRequiredDataNotFound(string message) : base(message){}

		public NL_ExceptionRequiredDataNotFound(string message, Exception inner)
			: base(message, inner){}
	}
	
}

