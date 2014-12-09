namespace Reports.StrategyRunningTime {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class StrategyRunningTime {

		public StrategyRunningTime(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		public KeyValuePair<ReportQuery, DataTable> Run(Report report, DateTime from, DateTime to) {
			m_oAsyncData = new SortedDictionary<int, StrategyData>();
			m_oSyncData = new SortedDictionary<int, StrategyData>();

			m_oDB.ForEachRowSafe(
				ProcessRow,
				"RptStrategyRunningTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("DateStart", from),
				new QueryParameter("DateEnd", to)
			);

			foreach (var pair in m_oAsyncData)
				pair.Value.CalculateTimes();

			foreach (var pair in m_oSyncData)
				pair.Value.CalculateTimes();

			var reprortQuery = new ReportQuery(report) {
				DateStart = from,
				DateEnd = to
			};

			return new KeyValuePair<ReportQuery, DataTable>(reprortQuery, ToTable());
		} // Run

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private SortedDictionary<int, StrategyData> m_oSyncData;
		private SortedDictionary<int, StrategyData> m_oAsyncData;

		private ActionResult ProcessRow(SafeReader sr, bool bRowsetStart) {
			SortedDictionary<int, StrategyData> oTarget = sr["IsSync"] ? m_oSyncData : m_oAsyncData;

			int nActionID = sr["ActionNameID"];

			if (oTarget.ContainsKey(nActionID))
				oTarget[nActionID].AddAction(sr);
			else
				oTarget[nActionID] = new StrategyData(nActionID, sr);

			return ActionResult.Continue;
		} // ProcessRow

		private DataTable ToTable() {
			var tbl = new DataTable();

			tbl.Columns.Add("Strategy", typeof (string));
			AddColumns(tbl, "SS"); // sync success
			AddColumns(tbl, "SF"); // sync fail
			AddColumns(tbl, "ST"); // sync total
			tbl.Columns.Add("SUnkCount", typeof (int)); // sync unknown count
			AddColumns(tbl, "AS"); // async success
			AddColumns(tbl, "AF"); // async fail
			AddColumns(tbl, "AT"); // async total
			tbl.Columns.Add("AUnkCount", typeof (int)); // async unknown count

			var oNames = new SortedSet<int>();

			oNames.UnionWith(m_oAsyncData.Select(p => p.Key));
			oNames.UnionWith(m_oSyncData.Select(p => p.Key));

			var oEmpty = new StrategyData(0, null);

			foreach (var nActionNameID in oNames) {
				StrategyData async = m_oAsyncData.ContainsKey(nActionNameID) ? m_oAsyncData[nActionNameID] : null;
				StrategyData sync = m_oSyncData.ContainsKey(nActionNameID) ? m_oSyncData[nActionNameID] : null;

				var row = new List<object> {
					sync == null ? async.Name : sync.Name
				};

				(sync ?? oEmpty).ToRow(row);
				(async ?? oEmpty).ToRow(row);

				tbl.Rows.Add(row.ToArray());
			} // for each

			return tbl;
		} // ToTable

		private static void AddColumns(DataTable tbl, string sPrefix) {
			tbl.Columns.Add(sPrefix + "Count", typeof (int));

			tbl.Columns.Add(sPrefix + "Min", typeof (double));
			tbl.Columns.Add(sPrefix + "MinTime", typeof (DateTime));

			tbl.Columns.Add(sPrefix + "Avg", typeof (double));
			tbl.Columns.Add(sPrefix + "Med", typeof (double));
			tbl.Columns.Add(sPrefix + "Pct75", typeof (double));
			tbl.Columns.Add(sPrefix + "Pct90", typeof (double));

			tbl.Columns.Add(sPrefix + "Max", typeof (double));
			tbl.Columns.Add(sPrefix + "MaxTime", typeof (DateTime));
		} // AddColumns

	} // class StrategyRunningTime
} // namespace
