namespace EzService.EzServiceImplementation {
	using System;
	using System.ServiceModel;
	using EzBob.Backend.Strategies.Exceptions;
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

		public ActionMetaData SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			return ExecuteSync<SaveVatReturnData>(null, null, nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths);
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
			try {
				LoadVatReturnRawData oRaw;

				var oResult = new VatReturnDataActionResult {
					VatReturnRawData = new VatReturnRawData[0],
					RtiTaxMonthRawData = new RtiTaxMonthRawData[0],
					Summary = new VatReturnSummary[0],
				};

				oResult.MetaData = ExecuteSync(out oRaw, null, null, nCustomerMarketplaceID);

				if (oResult.MetaData.Status == ActionStatus.Done) {
					oResult.RtiTaxMonthRawData = oRaw.RtiTaxMonthRawData;
					oResult.VatReturnRawData = oRaw.VatReturnRawData;

					LoadVatReturnSummary oSummary;

					oResult.MetaData = ExecuteSync(out oSummary, nCustomerID, null, nCustomerID, nCustomerMarketplaceID);
					oResult.Summary = oSummary.Summary;
				} // if

				return oResult;
			}
			catch (Exception e) {
				if (!(e is AStrategyException))
					Log.Alert(e, "Something went exceptional while executing LoadVatReturnFullData.");

				throw new FaultException(e.Message);
			} // try
		} // LoadVatReturnFullData

		#endregion method LoadVatReturnFullData

		#endregion sync
	} // class EzServiceImplementation
} // namespace EzService
