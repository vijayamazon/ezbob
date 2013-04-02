using System;
using EzBob.CommonLib.TimePeriodLogic;

namespace EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData
{
	public enum RangeMarkerType
	{
		Full, Partial, Temporary
	}

	public class TeraPeakDatabaseSellerDataItem : ITimeRangedData
	{
		public TeraPeakDatabaseSellerDataItem( DateTime startDate, DateTime endDate )
		{
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

		public RangeMarkerType RangeMarker { get; set; }

		public bool InRange(DateTime fromDate, DateTime toDate)
		{
			return fromDate <= LeftBoundary && RightBoundary <= toDate;
		}

		public bool Include(DateTime fromDate, DateTime toDate)
		{
			return LeftBoundary <= fromDate && RightBoundary >= toDate;
		}

		public DateTime LeftBoundary
		{
			get { return StartDate; }
		}

		public DateTime RightBoundary
		{
			get { return EndDate; }
		}		

		public bool Include(DateTime recordTime)
		{
			return LeftBoundary <= recordTime && RightBoundary >= recordTime;
		}

		public override string ToString()
		{
			return string.Format( "[ {0}; {1} ]", StartDate, EndDate );
		}
	}
}