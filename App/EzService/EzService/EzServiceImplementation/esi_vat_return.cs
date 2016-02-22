namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Strategies.VatReturn;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		public ActionMetaData BackfillLinkedHmrc() {
			return Execute<BackfillLinkedHmrc>(null, null);
		} // BackfillLinkedHmrc

		public ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			return Execute<CalculateVatReturnSummary>(null, null, nCustomerMarketplaceID);
		} // CalculateVatReturnSummary

		public ActionMetaData AndRecalculateVatReturnSummaryForAll() {
			return Execute<AndRecalculateVatReturnSummaryForAll>(null, null);
		} // AndRecalculateVatReturnSummaryForAll

		public VatReturnDataActionResult LoadVatReturnSummary(int nCustomerID, int nMarketplaceID) {
			LoadVatReturnSummary oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerID, nMarketplaceID);

			return new VatReturnDataActionResult {
				MetaData = oMetaData,
				Summary = oInstance.Summary,
			};
		} // LoadVatReturnSummary

		public VatReturnPeriodsActionResult LoadManualVatReturnPeriods(int nCustomerID) {
			LoadManualVatReturnPeriods oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID);

			return new VatReturnPeriodsActionResult {
				MetaData = oMetaData,
				Periods = oInstance.Periods.ToArray(),
			};
		} // LoadManualVatReturnPeriods

		public ElapsedTimeInfoActionResult SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			SaveVatReturnData oInstance;

			ActionMetaData oMetaData = ExecuteSync(
				out oInstance,
				null,
				null,
				nCustomerMarketplaceID,
				nHistoryRecordID,
				nSourceID,
				oVatReturn,
				oRtiMonths
			);

			return new ElapsedTimeInfoActionResult {
				MetaData = oMetaData,
				Value = oInstance.ElapsedTimeInfo,
			};
		} // SaveVatReturnData

		public VatReturnDataActionResult LoadVatReturnRawData(int nCustomerMarketplaceID) {
			LoadVatReturnRawData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerMarketplaceID);

			return new VatReturnDataActionResult {
				MetaData = oMetaData,
				RtiTaxMonthRawData = oInstance.RtiTaxMonthRawData,
				VatReturnRawData = oInstance.VatReturnRawData,
			};
		} // LoadVatReturnRawData

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

		public ActionMetaData RemoveManualVatReturnPeriod(Guid oPeriodID) {
			return ExecuteSync<RemoveManualVatReturnPeriod>(null, null, oPeriodID);
		} // /RemoveManualVatReturnPeriod

		public ActionMetaData UpdateLinkedHmrcPassword(
			string sCustomerID,
			string sDisplayName,
			string sPassword
		) {
			return ExecuteSync<UpdateLinkedHmrcPassword>(null, null, sCustomerID, sDisplayName, sPassword);
		} // UpdateLinkedHmrcPassword

		public StringActionResult ValidateAndUpdateLinkedHmrcPassword(
			string sCustomerID,
			string sDisplayName,
			string sPassword
		) {
			ValidateAndUpdateLinkedHmrcPassword oInstanse;

			ActionMetaData oMetaData = ExecuteSync(out oInstanse, null, null, sCustomerID, sDisplayName, sPassword);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstanse.ErrorMessage,
			};
		} // ValidateAndUpdateLinkedHmrcPassword
	} // class EzServiceImplementation
} // namespace EzService
