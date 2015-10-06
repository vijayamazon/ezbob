namespace Ezbob.Backend.Models {
	using System;

	public class MarketplaceTurnoverModel {
		public virtual long AggID { get; set; }
		public virtual DateTime TheMonth { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual DateTime UpdatingEnd { get; set; }
		public virtual int CustomerMarketPlaceUpdatingHistoryID { get; set; }
		public virtual int CustomerMarketPlaceID { get; set; }

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
