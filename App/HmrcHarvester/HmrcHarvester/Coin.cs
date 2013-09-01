namespace Ezbob.HmrcHarvester {
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

		#region method ToString

		public override string ToString() {
			return string.Format("{0} {1}", Amount, CurrencyCode);
		} // ToString

		#endregion method ToString

		#endregion public
	} // class Coin

	#endregion class Coin
} // namespace Ezbob.HmrcHarvester
