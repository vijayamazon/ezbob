namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class Coin : IEquatable<Coin> {

		public Coin()
			: this(0, "GBP") {
		} // constructor

		public Coin(decimal nAmount, string sCurrency) {
			Amount = nAmount;
			CurrencyCode = sCurrency;
		} // constructor

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public string CurrencyCode { get; set; }

		public override bool Equals(object obj) {
			if (obj == null)
				return false;

			if (object.ReferenceEquals(this, obj))
				return true;

			if (GetType() != obj.GetType())
				return false;

			return Equals(obj as Coin);
		} // Equals

		public bool Equals(Coin obj) {
			if (obj == null)
				return false;

			if (object.ReferenceEquals(this, obj))
				return true;

			if (GetType() != obj.GetType())
				return false;

			return Amount.Equals(obj.Amount) && (string.Compare(CurrencyCode, obj.CurrencyCode, StringComparison.CurrentCulture) == 0);
		} // Equals

		public override int GetHashCode() {
			return Amount.GetHashCode();
		} // GetHashCode

		public override string ToString() {
			return string.Format("{0} {1}", Amount, CurrencyCode);
		} // ToString

	} // class Coin
} // namespace
