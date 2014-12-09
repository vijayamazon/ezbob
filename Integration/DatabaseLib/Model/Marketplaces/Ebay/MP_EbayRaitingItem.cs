using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayRaitingItem
	{
		public virtual int Id { get; set; }
		public virtual MP_EbayFeedback EbayFeedback { get; set; }
		public virtual MP_AnalysisFunctionTimePeriod TimePeriod { get; set; }

		public virtual EbayRaitingInfo ItemAsDescribed { get; set; }
		public virtual EbayRaitingInfo Communication { get; set; }
		public virtual EbayRaitingInfo ShippingTime { get; set; }
		public virtual EbayRaitingInfo ShippingAndHandlingCharges { get; set; }
	}
}
