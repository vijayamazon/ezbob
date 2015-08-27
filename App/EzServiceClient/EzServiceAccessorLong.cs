namespace ServiceClientProxy {
	using System;
	using System.Collections.Generic;
	using EzServiceAccessor;
	using EzServiceReference;
	using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.Experian;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils;

	public class EzServiceAccessorLong : IEzServiceAccessor {
		public EzServiceAccessorLong() {
			m_oServiceClient = new ServiceClient();
		} // constructor

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

		public ExperianConsumerData ParseExperianConsumer(long nServiceLogId) {
			var res = m_oServiceClient.Instance.ParseExperianConsumer(nServiceLogId);
			return res.Value;
		}

		public ExperianConsumerData LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId) {
			var res = m_oServiceClient.Instance.LoadExperianConsumer(userId, customerId, directorId, nServiceLogId);
			return res.Value;
		}

		public ExperianLtd ParseExperianLtd(long nServiceLogID) {
			ExperianLtdActionResult ar = m_oServiceClient.Instance.ParseExperianLtd(nServiceLogID);
			return ar.Value;
		} // ParseExperianLtd

		public ExperianLtd LoadExperianLtd(long nServiceLogID) {
			ExperianLtdActionResult ar = m_oServiceClient.Instance.LoadExperianLtd(nServiceLogID);
			return ar.Value;
		} // LoadExperianLtd

		public ExperianLtd CheckLtdCompanyCache(int userId, string sCompanyRefNum) {
			ExperianLtdActionResult ar = m_oServiceClient.Instance.CheckLtdCompanyCache(userId, sCompanyRefNum);
			return ar.Value;
		} // CheckLtdCompanyCache

		public void EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail) {
			Dictionary<string, string> arg = new Dictionary<string, string>();

			foreach (var pair in oErrorsToEmail)
				arg[pair.Key] = pair.Value;

			m_oServiceClient.Instance.EmailHmrcParsingErrors(nCustomerID, nCustomerMarketplaceID, arg);
		} // EmailHmrcParsingErrors

		public CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
			return m_oServiceClient.Instance.GetCompanyDataForCreditBureau(underwriterId, refNumber).Result;
		}

        public WriteToLogPackage.OutputData ServiceLogWriter(WriteToLogPackage package)
        {
	        //should not execute this strategy from web (only from service)
            this.m_oServiceClient.Instance.WriteToServiceLog(package.In.CustomerID, package.In.CustomerID, package.In);
            return null;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nlModel"></param>
		/// <returns></returns>
		public NL_Model AddPayment(NL_Model nlModel) {
			Console.WriteLine("========================!!!!!!!!!!!!ACCESSOR LONG!!!!!!!!!!!=============================");
			var result = this.m_oServiceClient.Instance.AddPayment(nlModel);
/*			Console.WriteLine("result====>" + result.Payment.PaymentID);
			Console.WriteLine("result== PaypointTransactionID==>" + result.PaypointTransaction.PaypointTransactionID);*/
			return result;
		}
	

		private readonly ServiceClient m_oServiceClient;
	} // class EzServiceAccessorLong
} // namespace ServiceClientProxy
