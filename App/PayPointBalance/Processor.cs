namespace PayPointBalance {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using PayPoint;

	public class Processor : SafeLog {
		public Processor(Conf cfg, DateTime oDate, ASafeLog oLog = null) : base(oLog) {
			m_oDate = oDate;

			this.cfg = cfg;

			Debug("\n\n***\n*** PayPointBalance.Processor configuration\n***\n");

			Debug("Date: {0}.", m_oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

			foreach (var ac in this.cfg.Accounts)

			Debug("{0}.", ac);

			Debug("\n\n***\n*** End of PayPointBalance.Processor configuration\n***\n");
		} // constructor

		public bool Init() {
			Debug("PayPointBalance.Init() started...");

			m_oDB = new SqlConnection(this);

			m_sPaypointCondition = m_oDate.ToString(DateFormat);

			Debug("PayPointBalance.Init() complete.");
			return true;
		} // Init

		public void Run() {
			Debug("PayPointBalance.Run() started...");

			try {
				DeleteOldData(m_oDB, m_oDate);
			}
			catch (Exception e) {
				Error(e, "Something went terribly wrong while deleting old PayPoint data.");
			} // try

			foreach (var account in this.cfg.Accounts) {
				try {
					Save(m_oDB, Fetch(m_sPaypointCondition, account), LoadColumns(m_oDB));
				} catch (Exception e) {
					Error(e, "Something went terribly wrong.");
				} // try
			} // for each account

			Debug("PayPointBalance.Run() complete.");
		} // Run

		public void Done() {
			Debug("PayPointBalance.Done() started...");

			m_oDB.Dispose();

			Debug("PayPointBalance.Done() complete.");
		} // Done

		private void DeleteOldData(AConnection oDB, DateTime oDate) {
			Debug("PayPointBalance.DeleteOldData started...");

			oDB.ExecuteNonQuery(
				DeleteOldDataStoredProc.Name,
				CommandSpecies.StoredProcedure,
				DeleteOldDataStoredProc.Arg(oDate)
			);

			Debug("PayPointBalance.DeleteOldData complete.");
		} // DeleteOldData

		private PayPointDataSet.TransactionDataTable Fetch(string sCondition, Conf.Account account) {
			Debug(
				"PayPointBalance.Fetch started using condition '{0}' from PayPoint account {1}...",
				sCondition,
				account.Mid
			);

			PayPointDataSet.TransactionDataTable tbl = null;

			for (int r = 1; r <= account.RetryCount; r++) {
				Debug("Attempt {0}: fetching data from PayPoint account {1}...", r, account.Mid);

				try {
					tbl = PayPointConnector.GetOrdersDragonStyle(
						sCondition,
						account.Mid,
						account.VpnPassword,
						account.RemotePassword
					);

					Info(
						"Attempt {1}: {0} fetched from PayPoint account {2}.",
						Grammar.Number(tbl.Rows.Count, "row"),
						r,
						account.Mid
					);

					break;
				}
				catch (Exception e) {
					string sMsg = (r == account.RetryCount)
						? string.Empty
						: string.Format("\n\nRetrying in {0} seconds...", account.SleepInterval);

					Error(
						e,
						"Attempt {0}: failed to fetch data from PayPoint account {1} with exception {2}",
						r,
						account.Mid,
						sMsg
					);

					System.Threading.Thread.Sleep(account.SleepInterval * 1000);
				} // try
			} // for

			Say(
				tbl == null ? Severity.Error : Severity.Debug,
				"PayPointBalance.Fetch {0} from PayPoint account {1}.",
				tbl == null ? "failed" : "complete",
				account.Mid
			);

			return tbl;
		} // Fetch

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

		private List<string> LoadColumns(AConnection oDB) {
			Debug("PayPointBalance.LoadColumns started...");

			// List<string> oRes = (from DataRow row in tbl.Rows select row[0].ToString()).ToList();

			/*
			var oRes = new List<string>();

			oDB.ForEachRowSafe((sr, bRowsetStart) => {
				oRes.Add(sr[0]);
				return ActionResult.Continue;
			}, LoadColumnsStoredProc.Name, CommandSpecies.StoredProcedure);
			*/

			List<string> oRes = (
				from SafeReader sr
					in oDB.ExecuteEnumerable(LoadColumnsStoredProc.Name, CommandSpecies.StoredProcedure)
				select (string)sr[0]
			).ToList();

			Info("{0} found.", Grammar.Number(oRes.Count, "count"));

			Debug("PayPointBalance.LoadColumns complete.");
			return oRes;
		} // LoadColumns

		private static class DeleteOldDataStoredProc {
			public const string Name = "DeleteOldPayPointBalanceData";

			public static QueryParameter Arg(DateTime oDate) {
				return new QueryParameter("@Date", oDate.ToString(DateFormat));
			} // Arg
		} // DeleteOldDataStoredProc

		private static class InsertDataStoredProc {
			public const string Name = "InsertPayPointData";
		} // InsertDataStoredProc

		private static class LoadColumnsStoredProc {
			public const string Name = "LoadPayPointBalanceColumns";
		} // LoadColumnsStoredProc

		private string m_sPaypointCondition;
		private DateTime m_oDate;
		private AConnection m_oDB;

		private readonly Conf cfg;

		private const string DateFormat = "yyyyMMdd";

	} // class Processor

} // namespace PayPointBalance
