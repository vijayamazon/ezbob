using System;
using System.Collections.Generic;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData
{
	public class DatabaseEbayFeedbackData
	{
		public DatabaseEbayFeedbackData( DateTime submitted )
		{
			Submitted = submitted;
			FeedbackByPeriod = new Dictionary<TimePeriodEnum, DatabaseEbayFeedbackDataByPeriod>();
			RaitingByPeriod = new Dictionary<TimePeriodEnum, DatabaseEbayRaitingDataByPeriod>();
		}

		public DateTime Submitted { get; private set; }

		public int RepeatBuyerCount { get; set; }
		public double RepeatBuyerPercent { get; set; }
		public double TransactionPercent { get; set; }
		public int UniqueBuyerCount { get; set; }
		public int UniqueNegativeCount { get; set; }
		public int UniquePositiveCount { get; set; }
		public int UniqueNeutralCount { get; set; }

		public Dictionary<TimePeriodEnum, DatabaseEbayFeedbackDataByPeriod> FeedbackByPeriod { get; private set; }
		public Dictionary<TimePeriodEnum, DatabaseEbayRaitingDataByPeriod> RaitingByPeriod { get; private set; }
	}
}