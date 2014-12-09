using System;
using System.Xml.Serialization;
using EzBob.TeraPeakServiceLib.Requests.SellerResearch;

namespace EzBob.TeraPeakServiceLib
{
	[XmlRoot( "GetSellerResearchResults" )]
	public class GetSellerResearchResults
	{
		[XmlIgnore]
		public DateTime? Timestamp { get; set; }

		[XmlElement( "Timestamp" )]
		public string TimestampString
		{
			get { return DateTimeCorrectionHelper.TransformToSting( Timestamp ); }
			set { Timestamp = DateTimeCorrectionHelper.TransformDateValue( value ); }
		}

		public double ProcessingTime { get; set; }

		public int CallsRemaining { get; set; }

		[XmlIgnore]
		public DateTime? CallLimitResetTime { get; set; }

		[XmlElement( "CallLimitResetTime" )]
		public string CallLimitResetTimeString
		{
			get { return DateTimeCorrectionHelper.TransformToSting( CallLimitResetTime ); }
			set { CallLimitResetTime = DateTimeCorrectionHelper.TransformDateValue( value ); }
		}

		public ModifiedDateQuery ModifiedQuery { get; set; }

		[XmlElement( "SearchResults" )]
		public SearchQueryResult SearchResults { get; set; }

		[XmlElement( "Errors" )]
		public QueryResultError[] Errors { get; set; }

		[XmlElement( "ApiAccountInfo" )]
		public QueryResultApiInfo ApiAccountInfo { get; set; }

		public int GetCallsRemaining()
		{
			return ApiAccountInfo != null ? ApiAccountInfo.CallsRemaining : CallsRemaining;
		}

		public bool HasError
		{
			get { return Errors != null; }
		}
	}

	public class ModifiedDateQuery
	{
		public ModifiedDateRange Dates { get; set; }
	}

	public class ModifiedDateRange
	{
		public ModifiedDateRange()
		{
		}

		[XmlIgnore]
		public DateTime? StartDate { get; set; }

		[XmlElement( "StartDate" )]
		public string StartDateString
		{
			get { return DateTimeCorrectionHelper.TransformToSting( StartDate ); }
			set 
			{ 
				StartDate = DateTimeCorrectionHelper.TransformDateValue( value );
				if ( StartDate.HasValue )
				{
					StartDate = StartDate.Value.Date;
				}
			}
		}

		[XmlIgnore]
		public DateTime? EndDate { get; set; }

		[XmlElement( "EndDate" )]
		public string EndDateString
		{
			get { return DateTimeCorrectionHelper.TransformToSting( EndDate ); }
			set 
			{ 
				EndDate = DateTimeCorrectionHelper.TransformDateValue( value );
				if ( EndDate.HasValue )
				{
					EndDate = EndDate.Value.Date;
				}
			}
		}

		public override string ToString()
		{
			return string.Format( "[ {0} - {1} ]", StartDate, EndDate );
		}

	}

	public class QueryResultApiInfo
	{
		public int CallsRemaining { get; set; }
	}

	public class SearchQueryResult
	{
		public QueryResultStatistic Statistics { get; set; }
        public Category[] Categories { get; set; }
	}

    public class QueryResultError
	{
		[XmlElement("Error")]
		public QueryResultErrorItem Error { get; set; }
	}

	public class QueryResultErrorItem
	{
		[XmlAttribute( "id" )]
		public int Id { get; set; }

		[XmlText]
		public string Error { get; set; }
	}

}
