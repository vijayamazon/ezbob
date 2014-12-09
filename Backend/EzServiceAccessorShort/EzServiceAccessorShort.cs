namespace EzServiceShortcut {
	using System.Collections.Generic;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.MailStrategies;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Backend.Strategies.VatReturn;
	using EzServiceAccessor;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class EzServiceAccessorShort : IEzServiceAccessor {

		static EzServiceAccessorShort() {
			ms_oLock = new object();
			ms_oLog = new SafeLog();
			ms_oDB = null;
		} // static constructor

		public static void Set(AConnection oDB, ASafeLog oLog) {
			lock (ms_oLock) {
				ms_oDB = oDB;
				ms_oLog = oLog ?? new SafeLog();
			} // lock
		} // Set

		public void CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			var stra = new CalculateVatReturnSummary(nCustomerMarketplaceID, ms_oDB, ms_oLog);
			stra.Execute();
		} // CalculateVatReturnSummary

		public ElapsedTimeInfo SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			var stra = new SaveVatReturnData(nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.ElapsedTimeInfo;
		} // CalculateVatReturnSummary

		public VatReturnFullData LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID) {
			var stra = new LoadVatReturnFullData(nCustomerID, nCustomerMarketplaceID, ms_oDB, ms_oLog);
			stra.Execute();

			return new VatReturnFullData {
				VatReturnRawData = stra.VatReturnRawData,
				RtiTaxMonthRawData = stra.RtiTaxMonthRawData,
				Summary = stra.Summary,
				BankStatement = stra.BankStatement,
				BankStatementAnnualized = stra.BankStatementAnnualized,
			};
		}

		public ExperianConsumerData ParseExperianConsumer(long nServiceLogID)
		{
			var stra = new ParseExperianConsumerData(nServiceLogID, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		}

		public ExperianConsumerData LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId)
		{
			var stra = new LoadExperianConsumerData(customerId, directorId, nServiceLogId, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		}

// LoadVatReturnFullData

		public ExperianLtd ParseExperianLtd(long nServiceLogID) {
			var stra = new ParseExperianLtd(nServiceLogID, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		} // ParseExperianLtd

		public ExperianLtd LoadExperianLtd(long nServiceLogID) {
			var stra = new LoadExperianLtd(null, nServiceLogID, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		} // LoadExperianLtd

		public ExperianLtd CheckLtdCompanyCache(int userId, string sCompanyRefNum) {
			var stra = new LoadExperianLtd(sCompanyRefNum, 0, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		} // CheckLtdCompanyCache

		public void EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail) {
			new EmailHmrcParsingErrors(nCustomerID, nCustomerMarketplaceID, oErrorsToEmail, ms_oDB, ms_oLog).Execute();
		} // EmailHmrcParsingErrors

		public CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
			GetCompanyDataForCreditBureau strategyInstance = new GetCompanyDataForCreditBureau(ms_oDB, ms_oLog, refNumber);

			strategyInstance.Execute();

			return new CompanyDataForCreditBureau {
				LastUpdate = strategyInstance.LastUpdate,
				Score = strategyInstance.Score,
				Errors = strategyInstance.Errors
			};
		}

		private static ASafeLog ms_oLog;
		private static AConnection ms_oDB;
		private static readonly object ms_oLock;

	} // class EzServiceAccessorShort
} // namespace EzServiceShortcut
