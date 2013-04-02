using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData
{
	public abstract class DataByPeriodBase
	{
		protected DataByPeriodBase( TimePeriodEnum timePeriod )
		{
			TimePeriod = timePeriod;
		}

		public TimePeriodEnum TimePeriod { get; private set; }
	}
}