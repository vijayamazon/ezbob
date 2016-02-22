namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionPaymentSave : Exception {

		public NL_ExceptionPaymentSave(){}

        public NL_ExceptionPaymentSave(string message) : base(message){}

		public NL_ExceptionPaymentSave(string message, Exception inner)
			: base(message, inner){}
	}
	
}

