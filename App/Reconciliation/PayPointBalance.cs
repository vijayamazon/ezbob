using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Ezbob.Database;
using Ezbob.Logger;
using PayPoint;

namespace Reconciliation {
	#region class PayPointBalance

	class PayPointBalance : SafeLog {
		#region public

		#region constructor

		public PayPointBalance(DateTime oDate, string sMid, string sVpnPassword, string sRemotePassword, ASafeLog oLog = null) : base(oLog) {
			m_oDate = oDate;
			m_sMid = sMid;
			m_sVpnPassword = sVpnPassword;
			m_sRemotePassword = sRemotePassword;
		} // constructor

		#endregion constructor

		#region method Init

		public bool Init() {
			Debug("PayPointBalance.Init() started...");

			m_oDB = new SqlConnection(this);

			m_sPaypointCondition = m_oDate.ToString(DateFormat);

			Info("Downloading PayPoint transactions for {0} using login {1}", m_sPaypointCondition, m_sMid);

			Debug("PayPointBalance.Init() complete.");
			return true;
		} // Init

		#endregion method Init

		#region method Run

		public void Run() {
			Debug("PayPointBalance.Run() started...");

			try {
				DeleteOldData(m_oDB, m_oDate);

				FetchAndSave(m_oDB, m_sPaypointCondition, m_sMid, m_sVpnPassword, m_sRemotePassword);
			}
			catch (Exception e) {
				Error("Failed to fetch PayPoint data: {0}", e);
			} // try

			Debug("PayPointBalance.Run() complete.");
		} // Run

		#endregion method Run

		#region method Done

		public void Done() {
			Debug("PayPointBalance.Done() started...");

			m_oDB.Dispose();

			Debug("PayPointBalance.Done() complete.");
		} // Done

		#endregion method Done

		#endregion public

		#region private

		#region method DeleteOldData

		private void DeleteOldData(AConnection oDB, DateTime oDate) {
			Debug("PayPointBalance.DeleteOldData started...");

			oDB.ExecuteNonQuery(
				DeleteOldDataStoredProc.Name,
				CommandSpecies.StoredProcedure,
				DeleteOldDataStoredProc.Arg(oDate)
			);

			Debug("PayPointBalance.DeleteOldData complete.");
		} // DeleteOldData

		#endregion method DeleteOldData

		#region method FetchAndSave

		private void FetchAndSave(AConnection oDB, string sCondition, string sMid, string sVpnPassword, string sRemotePassword) {
			Debug("PayPointBalance.FetchAndSave started...");

			List<string> aryColumns = LoadColumns(oDB);

			Debug("Fetching data from PayPoint...");

			PayPointDataSet.TransactionDataTable tbl = PayPointConnector.GetOrders(sCondition, sMid, sVpnPassword, sRemotePassword);

			Info("{0} row{1} fetched from PayPoint.", tbl.Rows.Count, tbl.Rows.Count == 1 ? "" : "s");

			int nCurRowIdx = 0;

			foreach (PayPointDataSet.TransactionRow row in tbl.Rows) {
				QueryParameter[] args = aryColumns.Select(sName => {
					PropertyInfo pi = row.GetType().GetProperty(sName);
					var obj = pi.GetGetMethod().Invoke(row, null);

					if (sName == "date") {
						DateTime result;
						obj = DateTime.TryParse(obj.ToString(), out result) ? result : (DateTime?)null;
					} // if

					return new QueryParameter("@" + sName, obj);
				}).ToArray();

				oDB.ExecuteNonQuery(InsertDataStoredProc.Name, CommandSpecies.StoredProcedure, args);

				Debug("Row {0} processing complete.", nCurRowIdx);
				nCurRowIdx++;
			} // foreach

			Debug("PayPointBalance.FetchAndSave complete.");
		} // FetchAndSave

		#endregion method DeleteOldData

		#region method LoadColumns

		private List<string> LoadColumns(AConnection oDB) {
			Debug("PayPointBalance.LoadColumns started...");

			DataTable tbl = oDB.ExecuteReader(LoadColumnsStoredProc.Name, CommandSpecies.StoredProcedure);

			List<string> oRes = (from DataRow row in tbl.Rows select row[0].ToString()).ToList();

			tbl.Dispose();

			Info("{0} columns found.", oRes.Count);

			Debug("PayPointBalance.LoadColumns complete.");
			return oRes;
		} // LoadColumns

		#endregion method LoadColumns

		#region class DeleteOldDataStoredProc

		private static class DeleteOldDataStoredProc {
			public const string Name = "DeleteOldPayPointBalanceData";

			public static QueryParameter Arg(DateTime oDate) {
				return new QueryParameter("@Date", oDate.ToString(DateFormat));
			} // Arg
		} // DeleteOldDataStoredProc

		#endregion class DeleteOldDataStoredProc

		#region class InsertDataStoredProc

		private static class InsertDataStoredProc {
			public const string Name = "InsertPayPointData";
		} // InsertDataStoredProc

		#endregion class InsertDataStoredProc

		#region class LoadColumnsStoredProc

		private static class LoadColumnsStoredProc {
			public const string Name = "LoadPayPointBalanceColumns";
		} // LoadColumnsStoredProc

		#endregion class LoadColumnsStoredProc

		#region fields

		private string m_sPaypointCondition;
		private DateTime m_oDate;
		private AConnection m_oDB;

		private string m_sMid;
		private string m_sVpnPassword;
		private string m_sRemotePassword;

		#endregion fields

		#region const

		private const string DateFormat = "yyyyMMdd";

		#endregion const

		#endregion private
	} // class PayPointBalance

	#endregion class PayPointBalance
} // namespace Reconciliation
