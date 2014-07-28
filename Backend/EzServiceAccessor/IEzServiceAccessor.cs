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

		/// <summary>
		/// Used to parse and store experian consumer data by ServiceLog
		/// </summary>
		/// <param name="nServiceLogID">MP_ServiceLog.Id</param>
		ExperianConsumerData ParseExperianConsumer(long nServiceLogID);

		/// <summary>
		/// Used to retrieve parsed experian consumer data 
		/// if service log id provided than by it,
		/// else if director id provided - last director check
		/// else last customer check (by customer id)
		/// </summary>
		/// <param name="customerId">Customer.Id</param>
		/// <param name="directorId">Director.Id</param>
		/// <param name="nServiceLogID">MP_ServiceLogId</param>
		/// <returns></returns>
		ExperianConsumerData LoadExperianConsumer(int customerId, int? directorId, long? nServiceLogID);
		

		ExperianLtd ParseExperianLtd(long nServiceLogID);

		ExperianLtd LoadExperianLtd(long nServiceLogID);
		ExperianLtd CheckLtdCompanyCache(string sCompanyRefNum);
	} // interface IEzServiceAccessor
} // namespace EzServiceAccessor
