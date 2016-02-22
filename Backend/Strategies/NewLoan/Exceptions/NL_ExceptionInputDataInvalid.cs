namespace Ezbob.Backend.Strategies.NewLoan.Exceptions {
	using System;

	public class NL_ExceptionInputDataInvalid : Exception {

		public static string DefaultMessage = "InputDataInvalid";

		public NL_ExceptionInputDataInvalid(){}

        public NL_ExceptionInputDataInvalid(string message) : base(message){}

		public NL_ExceptionInputDataInvalid(string message, Exception inner)
			: base(message, inner){}
	}
	
}