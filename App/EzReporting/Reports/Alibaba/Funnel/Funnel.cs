namespace Reports.Alibaba.Funnel {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using OfficeOpenXml;

	class Funnel : IAlibaba {

		// ReSharper disable UnusedParameter.Local
		// oDateEnd: for future use.
		public Funnel(ExcelPackage oExcel, string sBatchName, DateTime? oDateEnd, AConnection oDB, ASafeLog oLog) {
			if (oDB == null)
				throw new Exception("Database connection not specified for Funnel report.");

			m_oDB = oDB;

			m_oLog = oLog ?? new SafeLog();

			Report = oExcel;

			m_sBatchName = sBatchName;
		} // constructor
		// ReSharper restore UnusedParameter.Local

		public void Generate() {
			m_oFunnel = new List<FunnelRow>();
			m_oRejectReasons = new List<RejectReasonRow>();
			m_nRejectReasonTotal = 0;

			m_oDB.ForEachRowSafe(
				ProcessRow,
				"RptAlibabaFunnel",
				CommandSpecies.StoredProcedure,
				new QueryParameter("BatchName", m_sBatchName)
			);

			foreach (RejectReasonRow rrr in m_oRejectReasons)
				rrr.Pct = m_nRejectReasonTotal < 0.8 ? 0 : rrr.Counter / m_nRejectReasonTotal;

			for (int i = 0; i < m_oFunnel.Count; i++) {
				FunnelRow oCur = m_oFunnel[i];

				if (!oCur.DoDropOff)
					break;

				FunnelRow oNext = m_oFunnel[i + 1];

				oCur.DropOff = oCur.Counter - oNext.Counter;
				oCur.Pct = oCur.Counter < 1 ? 0 : (double)oNext.Counter / (double)oCur.Counter;
			} // for

			string sPrefix = (m_sBatchName ?? "Total") + " ";

			CreateOneSheet(m_oFunnel, sPrefix + "Funnel", "Funnel", "Unique page views", "Drop off", "Conversion");
			CreateOneSheet(m_oRejectReasons, sPrefix + "Decline reasons", "Decline reason", "Count", "% of total");
		} // Generate

		public ExcelPackage Report { get; private set; }

		private void CreateOneSheet<T>(IEnumerable<T> oData, string sSheetName, params string[] aryColumnNames) where T : StrInt {
			ExcelWorksheet oSheet = Report.CreateSheet(sSheetName, false, aryColumnNames);

			foreach (T oRow in oData)
				oRow.SaveTo(oSheet);

			oSheet.View.ShowGridLines = false;
		} // CreateOneSheetName

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private void ProcessRow(SafeReader sr) {
			string sRowType = sr["RowType"];

			RowTypes nRowType;

			if (!Enum.TryParse(sRowType, out nRowType)) {
				m_oLog.Alert("Failed to parse DataSharing row type '{0}'.", sRowType);
				return;
			} // if

			switch (nRowType) {
			case RowTypes.Funnel:
				m_oFunnel.Add(sr.Fill<FunnelRow>());
				break;

			case RowTypes.RejectReason:
				RejectReasonRow rrr = sr.Fill<RejectReasonRow>();
				m_oRejectReasons.Add(rrr);
				m_nRejectReasonTotal += rrr.Counter;
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessRow

		private List<FunnelRow> m_oFunnel;
		private List<RejectReasonRow> m_oRejectReasons;

		private readonly string m_sBatchName;

		private double m_nRejectReasonTotal;

		private enum RowTypes {
			Funnel,
			RejectReason,
		} // enum RowTypes

	} // class Funnel
} // namespace
