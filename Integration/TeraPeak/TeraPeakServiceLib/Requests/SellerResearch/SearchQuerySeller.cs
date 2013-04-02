using System.Xml.Serialization;
using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	public class SearchQuerySeller : SearchQuery
	{
		public SearchQuerySeller()
		{
		}

		public SearchQuerySeller( string sellerId, SearchQueryDates dates, ResultSellerInfo resultSet)
		{
			ResultSet = resultSet;
			SellerFilters = new SellerInfo( sellerId );
			Dates = dates;
		}

		[XmlElement( "SellerFilters" )]
		public SellerInfo SellerFilters { get; set; }

		//[XmlElement( "ResultSet" )]
		public ResultSellerInfo ResultSet { get; set; }

		[XmlElement( "Dates" )]
		public SearchQueryDates Dates { get; set; }
	}
}