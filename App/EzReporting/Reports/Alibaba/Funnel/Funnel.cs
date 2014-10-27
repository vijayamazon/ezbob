namespace Reports.Alibaba.Funnel {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	public class Funnel {
		#region public

		#region constructor

		public Funnel(DateTime? oDateEnd, AConnection oDB, ASafeLog oLog) {
			if (oDB == null)
				throw new Exception("Database connection not specified for Funnel report.");

			m_oLog = oLog ?? new SafeLog();
			m_oSp = new RptAlibabaFunnel(oDateEnd, oDB, oLog);
		} // constructor

		#endregion constructor

		#region method Generate

		public void Generate() {
			m_oEclFunnel = new List<StrInt>();
			m_oEzbobFunnel = new List<EzbobFunnelRow>();
			m_oRejectReasons = new List<RejectReasonRow>();
			m_nRejectReasonTotal = 0;

			m_oSp.ForEachRowSafe(ProcessRow);

			foreach (RejectReasonRow rrr in m_oRejectReasons)
				rrr.Pct = m_nRejectReasonTotal < 0.8 ? 0 : rrr.Counter / m_nRejectReasonTotal;

			for (int i = 0; i < m_oEzbobFunnel.Count - 1; i++) {
				EzbobFunnelRow oCur = m_oEzbobFunnel[i];
				EzbobFunnelRow oNext = m_oEzbobFunnel[i + 1];

				oCur.DropOff = oCur.Counter - oNext.Counter;
				oCur.Pct = oCur.Counter < 1 ? 0 : (double)oNext.Counter / (double)oCur.Counter;
			} // for

			Report = new ExcelPackage();

			CreateOneSheet(m_oEclFunnel, "ECL funnel", "ECL funnel", "Figure");
			CreateOneSheet(m_oEzbobFunnel, "EZBOB funnel", "EZBOB funnel", "Unique page views", "Drop off", "Conversion");
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

		private readonly RptAlibabaFunnel m_oSp;

		#region class RptAlibabaFunnel

		private class RptAlibabaFunnel : AStoredProcedure {
			public RptAlibabaFunnel(DateTime? oDateEnd, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				DateEnd = oDateEnd ?? DateTime.UtcNow.AddDays(1);
			} // constructor

			public override bool HasValidParameters() {
				return DateEnd > FirstDay;
			} // HasValidParameters

			[UsedImplicitly]
			public DateTime DateEnd { get; set; }

			private static readonly DateTime FirstDay = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
		} // class RptAlibabaFunnel

		#endregion class RptAlibabaFunnel

		#region method ProcessRow

		private void ProcessRow(SafeReader sr) {
			string sRowType = sr["RowType"];

			RowTypes nRowType;

			if (!Enum.TryParse(sRowType, out nRowType)) {
				m_oLog.Alert("Failed to parse DataSharing row type '{0}'.", sRowType);
				return;
			} // if

			switch (nRowType) {
			case RowTypes.EclFunnel:
				m_oEclFunnel.Add(sr.Fill<StrInt>());
				break;

			case RowTypes.EzbobFunnel:
				m_oEzbobFunnel.Add(sr.Fill<EzbobFunnelRow>());
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

		private List<StrInt> m_oEclFunnel;
		private List<EzbobFunnelRow> m_oEzbobFunnel;
		private List<RejectReasonRow> m_oRejectReasons;

		private double m_nRejectReasonTotal;

		#region enum RowTypes

		private enum RowTypes {
			EclFunnel,
			EzbobFunnel,
			RejectReason,
		} // enum RowTypes

		#endregion enum RowTypes

		#endregion private
	} // class Funnel
} // namespace
