﻿namespace ServiceClientProxy {
	using EzServiceAccessor;
	using EzServiceReference;
	using Ezbob.Backend.Models;
	using Ezbob.Utils;

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

		public ElapsedTimeInfo SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			ElapsedTimeInfoActionResult etiar = m_oServiceClient.Instance.SaveVatReturnData(nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths);

			return etiar.MetaData.Status == ActionStatus.Done ? etiar.Value : new ElapsedTimeInfo();
		} // CalculateVatReturnSummary

		#endregion method SaveVatReturnData

		#region method LoadVatReturnFullData

		public VatReturnFullData LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID) {
			VatReturnDataActionResult vrdar = m_oServiceClient.Instance.LoadVatReturnFullData(nCustomerID, nCustomerMarketplaceID);

			return new VatReturnFullData {
				VatReturnRawData = vrdar.VatReturnRawData,
				RtiTaxMonthRawData = vrdar.RtiTaxMonthRawData,
				Summary = vrdar.Summary,
				BankStatement = vrdar.BankStatement,
				BankStatementAnnualized = vrdar.BankStatementAnnualized,
			};
		} // LoadVatReturnFullData

		#endregion method LoadVatReturnFullData

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;

		#endregion private
	} // class EzServiceAccessorLong
} // namespace ServiceClientProxy
