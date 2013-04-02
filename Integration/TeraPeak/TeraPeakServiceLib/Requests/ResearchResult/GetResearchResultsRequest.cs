using System;
using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib.Requests.ResearchResult
{
	[Serializable]
	[XmlRoot( "GetResearchResults" )]
	public class GetResearchResultsRequest: ServiceRequestDataBase
	{
		public GetResearchResultsRequest()
			: this( null, (SearchQuery)null, DateTime.Now )
		{
			
		}

		public GetResearchResultsRequest( TeraPeakRequesterCredentials requesterCredentials, SearchQuery searchQuery, DateTime? endDate = null )
			:base(requesterCredentials)
		{
			SearchQuery = searchQuery;
			if ( endDate.HasValue )
			{
				Dates = new DateRangeInfo(endDate);
			}
		}

		public GetResearchResultsRequest( TeraPeakRequesterCredentials requesterCredentials, string keyword, DateTime? endDate = null )
			: this( requesterCredentials, new SearchQuery( keyword ), endDate )
		{

		}

		public SearchQuery SearchQuery { get; set; }
		public DateRangeInfo Dates { get; set; }
	}
}