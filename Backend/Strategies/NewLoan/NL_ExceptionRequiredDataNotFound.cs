namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class NL_ExceptionRequiredDataNotFound : Exception {

		public NL_ExceptionRequiredDataNotFound(){}

        public NL_ExceptionRequiredDataNotFound(string message) : base(message){}

		public NL_ExceptionRequiredDataNotFound(string message, Exception inner)
			: base(message, inner){}
	}
	
}

