namespace EZBob.DatabaseLib.Common {
	#region class Coin

	public class Coin {
		#region public

		#region constructor

		public Coin() : this(0, "GBP") {
		} // constructor

		public Coin(decimal nAmount, string sCurrency) {
			Amount = nAmount;
			CurrencyCode = sCurrency;
		} // constructor

		#endregion constructor

		#region property Amount

		public virtual decimal Amount { get; set; }

		#endregion property Amount

		#region property CurrencyCode

		public virtual string CurrencyCode { get; set; }

		#endregion property CurrencyCode

		#endregion public
	} // class Coin

	#endregion class Coin
} // namespace EZBob.DatabaseLib.Common
