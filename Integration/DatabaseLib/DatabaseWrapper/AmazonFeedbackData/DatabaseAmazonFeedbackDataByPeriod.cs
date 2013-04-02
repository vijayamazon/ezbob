using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.AmazonFeedbackData
{
	public class DatabaseAmazonFeedbackDataByPeriod : DataByPeriodBase
	{
		public DatabaseAmazonFeedbackDataByPeriod( TimePeriodEnum timePeriod )
			: base( timePeriod )
		{
		}

		public int? Count { get; set; }
		public int? Negative { get; set; }
		public int? Positive { get; set; }
		public int? Neutral { get; set; }
	}
}