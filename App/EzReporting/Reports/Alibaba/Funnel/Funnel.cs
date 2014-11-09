namespace Reports.Alibaba.Funnel {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using OfficeOpenXml;

	public class Funnel : IAlibaba {
		#region public

		#region constructor

		// ReSharper disable UnusedParameter.Local
		// oDateEnd: for future use.
		public Funnel(DateTime? oDateEnd, AConnection oDB, ASafeLog oLog) {
			if (oDB == null)
				throw new Exception("Database connection not specified for Funnel report.");

			m_oDB = oDB;

			m_oLog = oLog ?? new SafeLog();
		} // constructor
		// ReSharper restore UnusedParameter.Local

		#endregion constructor

		#region method Generate

		public void Generate() {
			m_oFunnel = new List<FunnelRow>();
			m_oRejectReasons = new List<RejectReasonRow>();
			m_nRejectReasonTotal = 0;

			m_oDB.ForEachRowSafe(ProcessRow, "RptAlibabaFunnel", CommandSpecies.StoredProcedure);

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

			Report = new ExcelPackage();

			CreateOneSheet(m_oFunnel, "Funnel", "Funnel", "Unique page views", "Drop off", "Conversion");
			CreateOneSheet(m_oRejectReasons, "Decline reasons", "Decline reason", "Count", "% of total");

			Report.AutoFitColumns();
		} // Generate

		#endregion method Generate

		public ExcelPackage Report { get; private set; }

		#endregion public

		#region private

		#region method CreateOneSheet

		private void CreateOneSheet<T>(IEnumerable<T> oData, string sSheetName, params string[] aryColumnNames) where T : StrInt {
			ExcelWorksheet oSheet = Report.CreateSheet(sSheetName, false, aryColumnNames);

			foreach (T oRow in oData)
				oRow.SaveTo(oSheet);

			oSheet.View.ShowGridLines = false;
		} // CreateOneSheetName

		#endregion method CreateOneSheet

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		#region method ProcessRow

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

		#endregion method ProcessRow

		private List<FunnelRow> m_oFunnel;
		private List<RejectReasonRow> m_oRejectReasons;

		private double m_nRejectReasonTotal;

		#region enum RowTypes

		private enum RowTypes {
			Funnel,
			RejectReason,
		} // enum RowTypes

		#endregion enum RowTypes

		#endregion private
	} // class Funnel
} // namespace
