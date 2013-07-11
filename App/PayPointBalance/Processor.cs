﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ezbob.Database;
using Ezbob.Logger;
using PayPoint;

namespace PayPointBalance {
	#region class Processor

	public class Processor : SafeLog {
		#region public

		#region constructor

		public Processor(DateTime oDate, ASafeLog oLog = null)
			: base(oLog) {
			m_oDate = oDate;
			m_sMid = Conf.PayPointMid;
			m_sVpnPassword = Conf.PayPointVpnPassword;
			m_sRemotePassword = Conf.PayPointRemotePassword;
			m_nRetryCount = Conf.PayPointRetryCount;
			m_nSleepInterval = Conf.PayPointSleepInterval;

			Debug("\n\n***\n*** PayPointBalance.Processor configuration\n***\n");

			Debug("Date: {0}.", m_oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));
			Debug("Mid: {0}.", m_sMid);
			Debug("VPN password: **************.");
			Debug("Remote password: ++++++++++++++.");
			Debug("Retry count: {0} time{1}.", m_nRetryCount, m_nRetryCount == 1 ? "" : "s");
			Debug("Sleep interval between retries: {0} seconds.", m_nSleepInterval);

			Debug("\n\n***\n*** End of PayPointBalance.Processor configuration\n***\n");
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

				Save(
					m_oDB,
					Fetch(m_sPaypointCondition, m_sMid, m_sVpnPassword, m_sRemotePassword),
					LoadColumns(m_oDB)
				);
			}
			catch (Exception e) {
				Error("Something went terribly wrong: {0}", e);
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

		#region method Fetch

		private PayPointDataSet.TransactionDataTable Fetch(string sCondition, string sMid, string sVpnPassword, string sRemotePassword) {
			Debug("PayPointBalance.Fetch started...");

			PayPointDataSet.TransactionDataTable tbl = null;

			for (int r = 1; r <= m_nRetryCount; r++) {
				Debug("Attempt {0}: fetching data from PayPoint...", r);

				try {
					tbl = PayPointConnector.GetOrdersDragonStyle(sCondition, sMid, sVpnPassword, sRemotePassword);
					Info("Attempt {2}: {0} row{1} fetched from PayPoint.", tbl.Rows.Count, tbl.Rows.Count == 1 ? "" : "s", r);
					break;
				}
				catch (Exception e) {
					string sMsg = (r == m_nRetryCount) ? string.Empty : string.Format("\n\nRetrying in {0} seconds...", m_nSleepInterval);
					Error("Attempt {0}: failed to fetch data from PayPoint with exception {1}{2}", r, e, sMsg);
					System.Threading.Thread.Sleep(m_nSleepInterval * 1000);
				} // try
			} // for

			Say(tbl == null ? Severity.Error : Severity.Debug, "PayPointBalance.Fetch {0}.", tbl == null ? "failed" : "complete");

			return tbl;
		} // Fetch

		#endregion method Fetch

		#region method Save

		private void Save(AConnection oDB, PayPointDataSet.TransactionDataTable tbl, List<string> aryColumns) {
			Debug("PayPointBalance.Save started...");

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

			Debug("PayPointBalance.Save complete.");
		} // Save

		#endregion method Save

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

		private readonly string m_sMid;
		private readonly string m_sVpnPassword;
		private readonly string m_sRemotePassword;
		private readonly int m_nRetryCount;
		private readonly int m_nSleepInterval;

		#endregion fields

		#region const

		private const string DateFormat = "yyyyMMdd";

		#endregion const

		#endregion private
	} // class Processor

	#endregion class Processor
} // namespace PayPointBalance
