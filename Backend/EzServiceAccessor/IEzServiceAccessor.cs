namespace EzServiceAccessor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils;

	public interface IEzServiceAccessor {

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
		/// 
		/// </summary>
		/// <param name="userId">Underwriter.Id</param>
		/// <param name="customerId">Customer.Id</param>
		/// <param name="directorId">Director.Id</param>
		/// <param name="nServiceLogID">MP_ServiceLogId</param>
		/// <returns></returns>
		ExperianConsumerData LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogID);

		ExperianLtd ParseExperianLtd(long nServiceLogID);

		ExperianLtd LoadExperianLtd(long nServiceLogID);
		ExperianLtd CheckLtdCompanyCache(int userId, string sCompanyRefNum);

		void EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail);

		CompanyData GetNonLimitedData(int underwriterId, string refNumber);

		CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber);

		WriteToLogPackage.OutputData ServiceLogWriter(WriteToLogPackage package);

		void AddPayment(int customerID, NL_Payments payment, int userID = 1);

		long GetLoanByOldID(int loanId, int customerID = 1, int userID = 1);
		void LinkPaymentToInvestor(int userID, int loanTransactionID, int loanID, int customerID, decimal amount, DateTime transactionDate);

		List<NL_Loans> GetCustomerLoans(int customerID, int userID = 1);

		NL_Model GetLoanState(int customerID, long loanID, DateTime utcNow, int userID, bool getCalculatorState = true);

		void AcceptRollover(int customerID, long loanID);

	} // interface IEzServiceAccessor
} // namespace EzServiceAccessor