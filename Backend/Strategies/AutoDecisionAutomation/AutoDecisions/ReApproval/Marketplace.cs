namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval {
	using System;
	using AutomationCalculator.ProcessHistory.ReApproval;
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

		public void Fill(NewMarketplace mp) {
			mp.Init(ID, Name, Type, AddTime);
		} // Fill
	} // class Marketplace
} // namespace
