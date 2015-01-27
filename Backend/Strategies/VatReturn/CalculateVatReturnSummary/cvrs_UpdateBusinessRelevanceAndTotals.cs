namespace Ezbob.Backend.Strategies.VatReturn {
	using Ezbob.Database;

	public partial class CalculateVatReturnSummary : AStrategy {
		private void UpdateBusinessRelevanceAndTotals() {
			new UpdateHmrcBusinessRelevance(this.m_nCustomerID).Execute();

			DB.ExecuteNonQuery(
				"UpdateMpTotalsHmrc",
				CommandSpecies.StoredProcedure,
				new QueryParameter("HistoryID")
			);
		} // UpdateBusinessRelevanceAndTotals
	} // class CalculateVatReturnSummary
} // namespace
