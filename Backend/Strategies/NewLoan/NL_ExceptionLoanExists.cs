namespace Ezbob.Backend.Strategies.NewLoan {
	using System;

	public class NL_ExceptionOfferNotValid : Exception {

		public NL_ExceptionOfferNotValid(){}

        public NL_ExceptionOfferNotValid(string message) : base(message){}

		public NL_ExceptionOfferNotValid(string message, Exception inner)
			: base(message, inner){}
	}
	
}

