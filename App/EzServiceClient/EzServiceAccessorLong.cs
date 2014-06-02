namespace ServiceClientProxy {
	using EzServiceAccessor;
	using Ezbob.Backend.Models;

	public class EzServiceAccessorLong : IEzServiceAccessor {
		#region public

		#region constructor

		public EzServiceAccessorLong() {
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region method CalculateVatReturnSummary

		public void CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			m_oServiceClient.Instance.CalculateVatReturnSummary(nCustomerMarketplaceID);
		} // CalculateVatReturnSummary

		#endregion method CalculateVatReturnSummary

		#region method SaveVatReturnData

		public void SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			m_oServiceClient.Instance.SaveVatReturnData(nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths);
		} // CalculateVatReturnSummary

		#endregion method SaveVatReturnData

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;

		#endregion private
	} // class EzServiceAccessorLong
} // namespace ServiceClientProxy
