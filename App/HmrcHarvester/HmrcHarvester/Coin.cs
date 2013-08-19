namespace Ezbob.HmrcHarvester {
	#region class Coin

	public class Coin {
		#region public

		#region constructor

		public Coin() : this(0, "GBP") {
		} // constructor

		public Coin(decimal nAmount, string sCurrency) {
			Amount = nAmount;
			Currency = sCurrency;
		} // constructor

		#endregion constructor

		#region property Amount

		public virtual decimal Amount { get; set; }

		#endregion property Amount

		#region property Currency

		public virtual string Currency { get; set; }

		#endregion property Currency

		#endregion public
	} // class Coin

	#endregion class Coin
} // namespace Ezbob.HmrcHarvester
