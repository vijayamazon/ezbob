namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class ReschedulingOutPaymentPerIntervalExcpetion : Exception {

		public ReschedulingOutPaymentPerIntervalExcpetion(){}

        public ReschedulingOutPaymentPerIntervalExcpetion(string message) : base(message){}

		public ReschedulingOutPaymentPerIntervalExcpetion(string message, Exception inner)
			: base(message, inner){}
	}
	
}

