namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using Database;

	public class MP_AmazonFeedbackItem
	{
		public virtual int Id { get; set; }
		public virtual MP_AmazonFeedback AmazonFeedback { get; set; }
		public virtual MP_AnalysisFunctionTimePeriod TimePeriod { get; set; }

		public virtual int? Count { get; set; }
		public virtual int? Negative { get; set; }
		public virtual int? Positive { get; set; }
		public virtual int? Neutral { get; set; }

	}
}