namespace ServiceClientProxy {
	using System.Collections.Generic;
	using EzServiceAccessor;
	using EzServiceReference;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
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

		public ExperianConsumerData ParseExperianConsumer(long nServiceLogId)
		{
			var res = m_oServiceClient.Instance.ParseExperianConsumer(nServiceLogId);
			return res.Value;
		}

		public ExperianConsumerData LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId)
		{
			var res = m_oServiceClient.Instance.LoadExperianConsumer(userId, customerId, directorId, nServiceLogId);
			return res.Value;
		}

		#region method ParseExperianLtd

		public ExperianLtd ParseExperianLtd(long nServiceLogID) {
			ExperianLtdActionResult ar = m_oServiceClient.Instance.ParseExperianLtd(nServiceLogID);
			return ar.Value;
		} // ParseExperianLtd

		#endregion method ParseExperianLtd

		#region method LoadExperianLtd

		public ExperianLtd LoadExperianLtd(long nServiceLogID) {
			ExperianLtdActionResult ar = m_oServiceClient.Instance.LoadExperianLtd(nServiceLogID);
			return ar.Value;
		} // LoadExperianLtd

		#endregion method LoadExperianLtd

		#region method CheckLtdCompanyCache

		public ExperianLtd CheckLtdCompanyCache(int userId, string sCompanyRefNum) {
			ExperianLtdActionResult ar = m_oServiceClient.Instance.CheckLtdCompanyCache(userId, sCompanyRefNum);
			return ar.Value;
		} // CheckLtdCompanyCache

		#endregion method CheckLtdCompanyCache

		#region method EmailHmrcParsingErrors

		public void EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail) {
			Dictionary<string, string> arg = new Dictionary<string, string>();

			foreach (var pair in oErrorsToEmail)
				arg[pair.Key] = pair.Value;

			m_oServiceClient.Instance.EmailHmrcParsingErrors(nCustomerID, nCustomerMarketplaceID, arg);
		} // EmailHmrcParsingErrors

		#endregion method EmailHmrcParsingErrors

		public CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
			return m_oServiceClient.Instance.GetCompanyDataForCreditBureau(underwriterId, refNumber).Result;
		}

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;

		#endregion private
	} // class EzServiceAccessorLong
} // namespace ServiceClientProxy
