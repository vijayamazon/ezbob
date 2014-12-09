namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Database;

	public class AndRecalculateVatReturnSummaryForAll : AStrategy {
		public override string Name {
			get { return "Recalculate VAT return summary for all"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nMarketplaceID = sr["MarketplaceID"];

					Log.Msg("Updating VAT return summary for marketplace {0}...", nMarketplaceID);

					try {
						new CalculateVatReturnSummary(nMarketplaceID).Execute();
						Log.Msg("Updated VAT return summary for marketplace {0}.", nMarketplaceID);
					}
					catch (Exception e) {
						Log.Alert(e, "Failed to update VAT return summary for marketplace {0}.", nMarketplaceID);
					} // try

					return ActionResult.Continue;
				},
				"LoadAllTheHmrcMarketplaces",
				CommandSpecies.StoredProcedure
			);
		} // Execute

	} // class AndRecalculateVatReturnSummaryForAll
} // namespace
