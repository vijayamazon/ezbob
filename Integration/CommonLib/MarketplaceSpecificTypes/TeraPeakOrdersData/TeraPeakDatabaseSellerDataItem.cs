namespace EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData {
	using System;
	using System.Collections.Generic;

	public enum RangeMarkerType {
		Full,
		Partial,
		Temporary
	}

	public class TeraPeakCategory {
		public int Id { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public int Level { get; set; }
		public int ParentCategoryID { get; set; }
	}

	public class CategoryStatistics {
		public int Listings { get; set; }
		public int Successful { get; set; }
		public int ItemsSold { get; set; }
		public double Revenue { get; set; }
		public double SuccessRate { get; set; }
		public TeraPeakCategory Category { get; set; }
	}

	public class TeraPeakDatabaseSellerDataItem {
		public TeraPeakDatabaseSellerDataItem(DateTime startDate, DateTime endDate) {
			StartDate = startDate;
			EndDate = endDate;
		}

		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }

		public double? Revenue { get; set; }
		public int? Listings { get; set; }
		public int? Transactions { get; set; }
		public int? Successful { get; set; }
		public int? Bids { get; set; }
		public int? ItemsOffered { get; set; }
		public int? ItemsSold { get; set; }
		public int? AverageSellersPerDay { get; set; }
		public double? SuccessRate { get; set; }

		public virtual List<CategoryStatistics> Categories { get; set; }

		public RangeMarkerType RangeMarker { get; set; }

		public override string ToString() {
			return string.Format("[ {0}; {1} ]", StartDate, EndDate);
		}
	}
}