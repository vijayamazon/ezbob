namespace EzService.EzServiceImplementation {
	using System;
	using EzBob.Backend.Strategies.VatReturn;
	using Ezbob.Backend.Models;

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

		public VatReturnDataActionResult LoadVatReturnSummary(int nCustomerID, int nMarketplaceID) {
			LoadVatReturnSummary oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerID, nMarketplaceID);

			return new VatReturnDataActionResult {
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

		#region method SaveVatReturnData

		public ElapsedTimeInfoActionResult SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			SaveVatReturnData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths);

			return new ElapsedTimeInfoActionResult {
				MetaData = oMetaData,
				Value = oInstance.ElapsedTimeInfo,
			};
		} // SaveVatReturnData

		#endregion method SaveVatReturnData

		#region method LoadVatReturnRawData

		public VatReturnDataActionResult LoadVatReturnRawData(int nCustomerMarketplaceID) {
			LoadVatReturnRawData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerMarketplaceID);

			return new VatReturnDataActionResult {
				MetaData = oMetaData,
				RtiTaxMonthRawData = oInstance.RtiTaxMonthRawData,
				VatReturnRawData = oInstance.VatReturnRawData,
			};
		} // LoadVatReturnRawData

		#endregion method LoadVatReturnRawData

		#region method LoadVatReturnFullData

		public VatReturnDataActionResult LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID) {
			LoadVatReturnFullData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID, nCustomerMarketplaceID);

			return new VatReturnDataActionResult {
				VatReturnRawData = oInstance.VatReturnRawData,
				RtiTaxMonthRawData = oInstance.RtiTaxMonthRawData,
				Summary = oInstance.Summary,
				BankStatement = oInstance.BankStatement,
				BankStatementAnnualized = oInstance.BankStatementAnnualized,
				MetaData = oMetaData,
			};
		} // LoadVatReturnFullData

		#endregion method LoadVatReturnFullData

		#region method RemoveManualVatReturnPeriod

		public ActionMetaData RemoveManualVatReturnPeriod(Guid oPeriodID) {
			return ExecuteSync<RemoveManualVatReturnPeriod>(null, null, oPeriodID);
		} // /RemoveManualVatReturnPeriod

		#endregion method RemoveManualVatReturnPeriod

		#endregion sync
	} // class EzServiceImplementation
} // namespace EzService
