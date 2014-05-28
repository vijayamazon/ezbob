namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.VatReturn;

	partial class EzServiceImplementation {
		#region async

		#region method CalculateVatReturnSummary

		public ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			return Execute<CalculateVatReturnSummary>(null, null, nCustomerMarketplaceID);
		} // CalculateVatReturnSummary

		#endregion method CalculateVatReturnSummary

		#region method AndRecalculateVatReturnSummaryForAll

		public ActionMetaData AndRecalculateVatReturnSummaryForAll() {
			return Execute<AndRecalculateVatReturnSummaryForAll>(null, null);
		} // AndRecalculateVatReturnSummaryForAll

		#endregion method AndRecalculateVatReturnSummaryForAll

		#endregion async

		#region sync

		#region method LoadVatReturnSummary

		public VatReturnSummaryActionResult LoadVatReturnSummary(int nCustomerID, int nMarketplaceID) {
			LoadVatReturnSummary oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerID, nMarketplaceID);

			return new VatReturnSummaryActionResult {
				MetaData = oMetaData,
				Summary = oInstance.Summary,
			};
		} // LoadVatReturnSummary

		#endregion method LoadVatReturnSummary

		#region method LoadManualVatReturnPeriods

		public VatReturnPeriodsActionResult LoadManualVatReturnPeriods(int nCustomerID) {
			LoadManualVatReturnPeriods oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID);

			return new VatReturnPeriodsActionResult {
				MetaData = oMetaData,
				Periods = oInstance.Periods.ToArray(),
			};
		} // LoadManualVatReturnPeriods

		#endregion method LoadManualVatReturnPeriods

		#endregion sync
	} // class EzServiceImplementation
} // namespace EzService
