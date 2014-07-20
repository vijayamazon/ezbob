namespace EzServiceShortcut {
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.VatReturn;
	using EzServiceAccessor;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class EzServiceAccessorShort : IEzServiceAccessor {
		#region static constructor

		static EzServiceAccessorShort() {
			ms_oLock = new object();
			ms_oLog = new SafeLog();
			ms_oDB = null;
		} // static constructor

		#endregion static constructor

		#region method Set

		public static void Set(AConnection oDB, ASafeLog oLog) {
			lock (ms_oLock) {
				ms_oDB = oDB;
				ms_oLog = oLog ?? new SafeLog();
			} // lock
		} // Set

		#endregion method Set

		#region public

		#region method CalculateVatReturnSummary

		public void CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			var stra = new CalculateVatReturnSummary(nCustomerMarketplaceID, ms_oDB, ms_oLog);
			stra.Execute();
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
			var stra = new SaveVatReturnData(nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.ElapsedTimeInfo;
		} // CalculateVatReturnSummary

		#endregion method SaveVatReturnData

		#region method LoadVatReturnFullData

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
		} // LoadVatReturnFullData

		#endregion method LoadVatReturnFullData

		#region method ParseExperianLtd

		public ExperianLtd ParseExperianLtd(long nServiceLogID) {
			var stra = new ParseExperianLtd(nServiceLogID, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		} // ParseExperianLtd

		#endregion method ParseExperianLtd

		#region method LoadExperianLtd

		public ExperianLtd LoadExperianLtd(long nServiceLogID) {
			var stra = new LoadExperianLtd(null, nServiceLogID, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		} // LoadExperianLtd

		public ExperianLtd CheckLtdCompanyCache(string sCompanyRefNum) {
			var stra = new LoadExperianLtd(sCompanyRefNum, 0, ms_oDB, ms_oLog);
			stra.Execute();
			return stra.Result;
		} // CheckLtdCompanyCache

		#endregion method LoadExperianLtd

		#endregion public

		#region private

		private static ASafeLog ms_oLog;
		private static AConnection ms_oDB;
		private static readonly object ms_oLock;

		#endregion private
	} // class EzServiceAccessorShort
} // namespace EzServiceShortcut
