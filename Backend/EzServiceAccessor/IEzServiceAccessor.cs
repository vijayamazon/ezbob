namespace EzServiceAccessor {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
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

		ExperianLtd LoadExperianLtd(long nServiceLogID);
		ExperianLtd CheckLtdCompanyCache(string sCompanyRefNum);
	} // interface IEzServiceAccessor
} // namespace EzServiceAccessor
