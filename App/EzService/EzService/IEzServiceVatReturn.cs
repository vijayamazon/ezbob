namespace EzService {
	using System;
	using System.ServiceModel;
	using Ezbob.Backend.Models;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceVatReturn {
		[OperationContract]
		ActionMetaData BackfillLinkedHmrc();

		[OperationContract]
		ActionMetaData AndRecalculateVatReturnSummaryForAll();

		[OperationContract]
		ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID);

		[OperationContract]
		VatReturnDataActionResult LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID);

		[OperationContract]
		VatReturnDataActionResult LoadVatReturnRawData(int nCustomerMarketplaceID);

		[OperationContract]
		VatReturnDataActionResult LoadVatReturnSummary(int nCustomerID, int nMarketplaceID);

		[OperationContract]
		ElapsedTimeInfoActionResult SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		);

		[OperationContract]
		ActionMetaData RemoveManualVatReturnPeriod(Guid oPeriodID);

		[OperationContract]
		ActionMetaData UpdateLinkedHmrcPassword(string sCustomerID, string sDisplayName, string sPassword);

		[OperationContract]
		StringActionResult ValidateAndUpdateLinkedHmrcPassword(
			string sCustomerID,
			string sDisplayName,
			string sPassword
		);
	} // interface IEzServiceVatReturn
} // namespace
