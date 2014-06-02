namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	[DataContract]
	public class Coin {
		#region constructor

		public Coin() : this(0, "GBP") {
		} // constructor

		public Coin(decimal nAmount, string sCurrency) {
			Amount = nAmount;
			CurrencyCode = sCurrency;
		} // constructor

		#endregion constructor

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public string CurrencyCode { get; set; }

		#region method ToString

		public override string ToString() {
			return string.Format("{0} {1}", Amount, CurrencyCode);
		} // ToString

		#endregion method ToString
	} // class Coin
} // namespace
