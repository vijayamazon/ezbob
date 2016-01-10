namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class ReschedulingNotAllowedBadCustomerStatusException : Exception {

		public ReschedulingNotAllowedBadCustomerStatusException() { }

		public ReschedulingNotAllowedBadCustomerStatusException(string message) : base(message) { }

		public ReschedulingNotAllowedBadCustomerStatusException(string message, Exception inner)
			: base(message, inner){}
	}
	
}

