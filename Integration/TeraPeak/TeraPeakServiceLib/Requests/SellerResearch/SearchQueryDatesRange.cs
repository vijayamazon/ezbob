using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;

namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	public class SearchQueryDatesRangeListData : List<SearchQueryDatesRange>
	{
		public SearchQueryDatesRangeListData()
		{
		}

		public SearchQueryDatesRangeListData(SearchQueryDatesRange range)
		{
			if ( range == null )
			{
				return;
			}
			Add( range );
		}
	}

	[Serializable]
	public class SearchQueryDatesRange: SearchQueryDates
	{
		public SearchQueryDatesRange()
			:this(RangeMarkerType.Temporary)
		{

		}

		private SearchQueryDatesRange(RangeMarkerType rangeMarker)
		{
			RangeMarker = rangeMarker;
		}

		public SearchQueryDatesRange(DateTime startDate, DateTime endDate, RangeMarkerType rangeMarkerType)
			:this(rangeMarkerType)
		{
			if ( startDate > endDate )
			{
				StartDate = endDate;
				EndDate = startDate;
			}

			StartDate = startDate;
			EndDate = endDate;
		}

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		[XmlIgnore]
		public RangeMarkerType RangeMarker { get; set; }

		public override string ToString()
		{
			return string.Format( "[ {0} - {1} ]", StartDate, EndDate );
		}

	}
}
