namespace EzBob.Models.Marketplaces {
	public class MarketPlaceFeedbackModel {
		public decimal RaitingPercent { get; set; }
		public int? PositiveFeedbacks { get; set; }
		public int? NeutralFeedbacks { get; set; }
		public int? NegativeFeedbacks { get; set; }

		public double AmazonSelerRating { get; set; }
	}
}
