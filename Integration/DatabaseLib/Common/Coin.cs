namespace EZBob.DatabaseLib.Common {
	using System;

	[Serializable]
	public class Coin {

		public Coin() : this(0, "GBP") {
		} // constructor

		public Coin(decimal nAmount, string sCurrency) {
			Amount = nAmount;
			CurrencyCode = sCurrency;
		} // constructor

		public virtual decimal Amount { get; set; }

		public virtual string CurrencyCode { get; set; }

	} // class Coin

} // namespace EZBob.DatabaseLib.Common
