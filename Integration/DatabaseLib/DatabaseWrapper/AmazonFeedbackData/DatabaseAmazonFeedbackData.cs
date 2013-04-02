using System;
using System.Collections.Generic;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.AmazonFeedbackData
{
	public class DatabaseAmazonFeedbackData
	{
		public DatabaseAmazonFeedbackData( DateTime submitted )
		{
			Submitted = submitted;
			FeedbackByPeriod = new Dictionary<TimePeriodEnum, DatabaseAmazonFeedbackDataByPeriod>();
		}

		public DateTime Submitted { get; private set; }

		public double UserRaining { get; set; }

		public Dictionary<TimePeriodEnum, DatabaseAmazonFeedbackDataByPeriod> FeedbackByPeriod { get; private set; }
	}
}