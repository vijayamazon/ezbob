using System;
using EZBob.DatabaseLib;
using EzBob.CommonLib;

namespace EzBob.AmazonServiceLib.UserInfo
{
	public class AmazonUserRatingInfo
	{
		public double Rating { get; set; }
		public FeedbackHistoryInfo FeedbackHistory { get; set; }
		public DateTime SubmittedDate { get; set; }

	    public string Name { get; set; }

		public RequestsCounterData RequestsCounter { get; set; }

		public void IncrementRequests( string method = null, string details = null )
		{
			if ( RequestsCounter == null )
			{
				RequestsCounter = new RequestsCounterData();
			}
			RequestsCounter.IncrementRequests( method, details );
		}

		public int? GetFeedbackValue(FeedbackPeriod amazonTimePeriod, FeedbackType feedbackType)
		{
			if ( FeedbackHistory == null)
			{
				return null;
			}
			
			return FeedbackHistory.GetFeedbackHistoryValue(amazonTimePeriod, feedbackType);
		}
	}
}