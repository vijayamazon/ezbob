namespace Ezbob.Backend.Models {
	using System;

	public class MarketplaceTurnoverModel {
		public long AggID { get; set; }
		public DateTime TheMonth { get; set; }
		public bool IsActive { get; set; }
		public decimal Turnover { get; set; }
		public DateTime UpdatingEnd { get; set; }
		public int CustomerMarketPlaceUpdatingHistoryID { get; set; }
		public int CustomerMarketPlaceID { get; set; }
		public int CustomerID { get; set; }
		public bool IsMarketplaceDisabled { get; set; }
		public Guid MarketplaceInternalID { get; set; }
		public bool IsPaymentAccount { get; set; }

		public override bool Equals(Object obj) {
			var t = obj as MarketplaceTurnoverModel;

			if (t == null)
				return false;

			return (
				CustomerMarketPlaceUpdatingHistoryID == t.CustomerMarketPlaceUpdatingHistoryID &&
				TheMonth == t.TheMonth
			);
		} // Equals

		public override int GetHashCode() {
			return (CustomerMarketPlaceUpdatingHistoryID + "|" + TheMonth).GetHashCode();
		} // GetHashCode

		public override string ToString() {
			return string.Join("|",
				AggID,
				CustomerMarketPlaceUpdatingHistoryID,
				CustomerMarketPlaceID,
				TheMonth,
				Turnover,
				UpdatingEnd
			);
		} // ToString
	} // class MarketplaceTurnoverModel
} // namespace
