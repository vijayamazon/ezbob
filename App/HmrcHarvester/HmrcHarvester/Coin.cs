namespace Ezbob.HmrcHarvester {

	public class Coin {

		public Coin() : this(0, "GBP") {
		} // constructor

		public Coin(decimal nAmount, string sCurrency) {
			Amount = nAmount;
			CurrencyCode = sCurrency;
		} // constructor

		public virtual decimal Amount { get; set; }

		public virtual string CurrencyCode { get; set; }

		public override string ToString() {
			return string.Format("{0} {1}", Amount, CurrencyCode);
		} // ToString

	} // class Coin

} // namespace Ezbob.HmrcHarvester
