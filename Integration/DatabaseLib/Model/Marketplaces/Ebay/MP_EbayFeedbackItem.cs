namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayFeedbackItem
	{
		public virtual int Id { get; set; }
		public virtual MP_EbayFeedback EbayFeedback { get; set; }
		public virtual MP_AnalysisFunctionTimePeriod TimePeriod { get; set; }

		public virtual int? Negative { get; set; }
		public virtual int? Positive { get; set; }
		public virtual int? Neutral { get; set; }
	}
}