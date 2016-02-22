namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionLoanForOldIDNotFound : Exception {
		
		public static string LoanNotFound = "NL loan for OldLoanID {0} not found";

		public NL_ExceptionLoanForOldIDNotFound() { }

		public NL_ExceptionLoanForOldIDNotFound(int loadLoanID) : base(string.Format(LoanNotFound, loadLoanID)) { }

		//public NL_ExceptionLoanForOldIDNotFound(string message, Exception inner): base(message, inner) { }
	}

}