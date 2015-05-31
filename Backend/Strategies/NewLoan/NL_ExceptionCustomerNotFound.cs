namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class NL_ExceptionCustomerNotFound : Exception {

		public NL_ExceptionCustomerNotFound(){}

        public NL_ExceptionCustomerNotFound(string message) : base(message){}

		public NL_ExceptionCustomerNotFound(string message, Exception inner)
			: base(message, inner){}
	}
	
}

