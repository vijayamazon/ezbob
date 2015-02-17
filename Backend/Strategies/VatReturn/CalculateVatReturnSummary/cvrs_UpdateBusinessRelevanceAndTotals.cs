namespace Ezbob.Backend.Strategies.VatReturn {
	using Ezbob.Database;

	public partial class CalculateVatReturnSummary : AStrategy {
		private void UpdateBusinessRelevanceAndTotals() {
			new UpdateHmrcBusinessRelevance(this.m_nCustomerID).Execute();

			if (this.historyRecordID > 0) {
				DB.ExecuteNonQuery(
					"UpdateMpTotalsHmrc",
					CommandSpecies.StoredProcedure,
					new QueryParameter("HistoryID", this.historyRecordID)
				);
			} // if
		} // UpdateBusinessRelevanceAndTotals
	} // class CalculateVatReturnSummary
} // namespace
