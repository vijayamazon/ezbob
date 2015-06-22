namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class ReschedulingSaveException : Exception {

		public ReschedulingSaveException() { }

		public ReschedulingSaveException(string message) : base(message) { }

		public ReschedulingSaveException(string message, Exception inner)
			: base(message, inner){}
	}
	
}

