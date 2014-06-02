namespace EzServiceAccessor {
	using Ezbob.Backend.Models;

	public interface IEzServiceAccessor {
		void CalculateVatReturnSummary(int nCustomerMarketplaceID);

		void SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		);
	} // interface IEzServiceAccessor
} // namespace EzServiceAccessor
