using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData
{
	public class DatabaseEbayRaitingDataByPeriod: DataByPeriodBase
	{
		public DatabaseEbayRaitingDataByPeriod( TimePeriodEnum timePeriod )
			:base(timePeriod)
		{			
		}

		public EbayRaitingInfo ItemAsDescribed { get; set; }
		public EbayRaitingInfo Communication { get; set; }
		public EbayRaitingInfo ShippingTime { get; set; }
		public EbayRaitingInfo ShippingAndHandlingCharges { get; set; }
	}
}
