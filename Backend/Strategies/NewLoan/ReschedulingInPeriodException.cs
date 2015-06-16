namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class ReschedulingInPeriodException : Exception {

		public ReschedulingInPeriodException() { }

		public ReschedulingInPeriodException(string message) : base(message) { }

		public ReschedulingInPeriodException(string message, Exception inner)
			: base(message, inner){}
	}
	
}

