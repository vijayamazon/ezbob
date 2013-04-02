using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData
{
	public class DatabaseEbayFeedbackDataByPeriod : DataByPeriodBase
	{
		public DatabaseEbayFeedbackDataByPeriod( TimePeriodEnum timePeriod ) : base(timePeriod)
		{
		}

		public int? Negative { get; set; }
		public int? Positive { get; set; }
		public int? Neutral { get; set; }
	}
}