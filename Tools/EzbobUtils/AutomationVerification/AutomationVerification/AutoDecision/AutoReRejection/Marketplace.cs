namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using Ezbob.Database;

	public class Marketplace {
		[FieldName("MarketplaceID")]
		public int ID { get; set; }

		[FieldName("MarketplaceName")]
		public string Name { get; set; }

		[FieldName("MarketplaceType")]
		public string Type { get; set; }

		[FieldName("MarketplaceAddTime")]
		public DateTime AddTime { get; set; }
	} // class Marketplace
} // namespace
