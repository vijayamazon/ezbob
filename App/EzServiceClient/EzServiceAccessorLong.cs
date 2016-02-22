namespace ServiceClientProxy {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils;
	using EzServiceAccessor;
	using log4net;
	using ServiceClientProxy.EzServiceReference;

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
		}// EmailHmrcParsingErrors

		public CompanyData GetNonLimitedData(int underwriterId, string refNumber) {

			CompanyData data = null;
			return data;//nothing will be returned since the function is never called this way
		}

		public CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
			return m_oServiceClient.Instance.GetCompanyDataForCreditBureau(underwriterId, refNumber).Result;
		}

		public WriteToLogPackage.OutputData ServiceLogWriter(WriteToLogPackage package) {
			//should not execute this strategy from web (only from service)
			this.m_oServiceClient.Instance.WriteToServiceLog(package.In.CustomerID, package.In.CustomerID, package.In);
			return null;
		}

		public void AddPayment(int customerID, NL_Payments payment, int userID) {
			this.m_oServiceClient.Instance.AddPayment(customerID, payment, userID);
		}

		public void LinkPaymentToInvestor(int userID, int loanTransactionID, int loanID, int customerID, decimal amount, DateTime transactionDate) {
			Log.InfoFormat("EzServiceAccessorLong LinkPaymentToInvestor {0} {1} {2} {3} {4} begin", loanTransactionID, loanID, customerID, amount, transactionDate);
			this.m_oServiceClient.Instance.LinkLoanRepaymentToInvestor(userID, customerID, loanID, loanTransactionID, amount, transactionDate);
		}

		public List<NL_Loans> GetCustomerLoans(int customerID, int userID) {
			return this.m_oServiceClient.Instance.GetCustomerLoans(customerID, userID).Value.ToList();
		}


		public NL_Model GetLoanState(int customerID, long loanID, DateTime utcNow, int userID, bool getCalculatorState = true) {
			var nlModel =  this.m_oServiceClient.Instance.GetLoanState(customerID, loanID, utcNow, userID, getCalculatorState).Value;
			return nlModel;
		}

		public long GetLoanByOldID(int loanId, int customerID = 1, int userID = 1) {
			return this.m_oServiceClient.Instance.GetLoanByOldID(loanId, customerID, userID).Value;
		}

		public void AcceptRollover(int customerID, long loanId) {
			this.m_oServiceClient.Instance.AcceptRollover(customerID, loanId);
		}

		private readonly ServiceClient m_oServiceClient;
		private static ILog Log = LogManager.GetLogger(typeof(EzServiceAccessorLong));
	} // class EzServiceAccessorLong
} // namespace ServiceClientProxy
