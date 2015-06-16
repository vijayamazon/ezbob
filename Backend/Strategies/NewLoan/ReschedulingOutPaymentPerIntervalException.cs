namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class ReschedulingOutPaymentPerIntervalException : Exception {

		public ReschedulingOutPaymentPerIntervalException() { }

		public ReschedulingOutPaymentPerIntervalException(string message) : base(message) { }

		public ReschedulingOutPaymentPerIntervalException(string message, Exception inner)
			: base(message, inner){}
	}
	
}

