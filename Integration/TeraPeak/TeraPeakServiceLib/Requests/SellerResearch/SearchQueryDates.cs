using System;
using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	//[XmlInclude( typeof( SearchQueryDatesFor ) )]
	//[XmlInclude( typeof( SearchQueryDatesCount ) )]
	//[XmlInclude( typeof( SearchQueryDatesCountWithEnd ) )]
	[XmlInclude( typeof( SearchQueryDatesRange ) )]
	public abstract class SearchQueryDates
	{
		/*public static SearchQueryDates Create( SearchQueryParamCountDates range )
		{
			return new SearchQueryDatesCount( range );
		}

		public static SearchQueryDates Create( DateTime startDate, DateTime endDate )
		{
			return new SearchQueryDatesRange( startDate, endDate );
		}

		public static SearchQueryDates Create( DateTime date )
		{
			return new SearchQueryDatesFor( date );
		}

		public static SearchQueryDates Create( SearchQueryParamCountDates range, DateTime endDate )
		{
			return new SearchQueryDatesCountWithEnd( range, endDate );
		}*/
	}
}