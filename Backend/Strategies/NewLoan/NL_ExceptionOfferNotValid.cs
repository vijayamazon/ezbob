namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class NL_ExceptionLoanExists : Exception {

		public NL_ExceptionLoanExists(){}

        public NL_ExceptionLoanExists(string message) : base(message){}

		public NL_ExceptionLoanExists(string message, Exception inner)
			: base(message, inner){}
	}
	
}

