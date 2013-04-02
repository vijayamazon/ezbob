using System;
using System.Xml.Serialization;
using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	[XmlRoot( "GetSellerResearchResults" )]
	public class GetSellerResearchResultsRequest : ServiceRequestDataBase
	{
		public GetSellerResearchResultsRequest() 
		{
		}

		/*public GetSellerResearchResultsRequest( TeraPeakRequesterCredentials requesterCredentials, string sellerId, DateTime startDate, DateTime endDate, ResultSellerInfo resultSellerInfo = null )
			: this( requesterCredentials, sellerId, resultSellerInfo, SearchQueryDates.Create( startDate, endDate ) )
		{
		}

		public GetSellerResearchResultsRequest( TeraPeakRequesterCredentials requesterCredentials, string sellerId, DateTime date, ResultSellerInfo resultSellerInfo = null )
			: this( requesterCredentials, sellerId, resultSellerInfo, SearchQueryDates.Create( date ) )
		{
		}

		public GetSellerResearchResultsRequest( TeraPeakRequesterCredentials requesterCredentials, string sellerId, SearchQueryParamCountDates range, ResultSellerInfo resultSellerInfo = null )
			: this( requesterCredentials, sellerId, resultSellerInfo, SearchQueryDates.Create( range ) )
		{
		}*/

		public GetSellerResearchResultsRequest( TeraPeakRequesterCredentials requesterCredentials, string sellerId, ResultSellerInfo resultSellerInfo = null, SearchQueryDates dates = null )
			:base(requesterCredentials)
		{		
			SearchQuery = new SearchQuerySeller( sellerId, dates, resultSellerInfo );
		}

		[XmlElement( "SearchQuery" )]
		public SearchQuerySeller SearchQuery { get; set; }
		
	}
}
