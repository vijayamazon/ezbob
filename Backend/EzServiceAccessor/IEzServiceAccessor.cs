namespace EzServiceAccessor {
	using Ezbob.Backend.Models;
	using Ezbob.Utils;

	public interface IEzServiceAccessor {
		void CalculateVatReturnSummary(int nCustomerMarketplaceID);

		ElapsedTimeInfo SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		);

		VatReturnFullData LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID);

		void ParseExperianLtd(long nServiceLogID);
	} // interface IEzServiceAccessor
} // namespace EzServiceAccessor
