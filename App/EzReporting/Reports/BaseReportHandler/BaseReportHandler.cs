namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Html;
	using Html.Attributes;
	using Html.Tags;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	using Cci;
	using TraficReport;

	public partial class BaseReportHandler : SafeLog {
		#region public

		#region constructor

		public BaseReportHandler(AConnection oDB, ASafeLog log = null)
			: base(log) {
			DB = oDB;
		} // constructor

		#endregion constructor

		#region HTML generators

		#region method TableReport

		public ATag TableReport(ReportQuery rptDef, bool isSharones = false, string sRptTitle = "", List<string> oColumnTypes = null) {
			return TableReport(rptDef, null, isSharones, sRptTitle, oColumnTypes);
		} // TableReport

		public ATag TableReport(ReportQuery rptDef, DataTable oReportData, bool isSharones = false, string sRptTitle = "", List<string> oColumnTypes = null) {
			int lineCounter = 0;
			var tbl = new Table().Add<Class>("Report");
			Tbody oTbody;

			try {
				if (!isSharones)
					tbl.Add<ID>("tableReportData");
			}
			catch (Exception e) {
				Alert(e, "Failed to add HTML id to report table.");
				return tbl;
			} // try

			try {
				var tr = new Tr().Add<Class>("HR");

				for (int columnIndex = 0; columnIndex < rptDef.Columns.Length; columnIndex++)
					if (rptDef.Columns[columnIndex].IsVisible)
						tr.Append(new Th().Add<Class>("H").Append(new Text(rptDef.Columns[columnIndex].Caption)));

				tbl.Append(new Thead().Append(tr));
			}
			catch (Exception e) {
				Alert(e, "Failed to initialise table header row.");
				return tbl;
			} // try

			try {
				oTbody = new Tbody();
				tbl.Append(oTbody);

				if (oColumnTypes != null) {
					oColumnTypes.Clear();

					for (int columnIndex = 0; columnIndex < rptDef.Columns.Length; columnIndex++)
						oColumnTypes.Add("string");
				} // if
			}
			catch (Exception e) {
				Alert(e, "Failed to initialise report table column types.");
				return tbl;
			} // try

			if (oReportData == null) {
				try {
					rptDef.Execute(DB, (sr, bRowsetStart) => {
						ProcessTableReportRow(rptDef, sr, oTbody, lineCounter, oColumnTypes);
						lineCounter++;
						return ActionResult.Continue;
					}); // for each data row
				}
				catch (Exception e) {
					Alert(e, "Failed to fetch data from DB or create report table body.");
					return tbl;
				} // try
			}
			else {
				try {
					foreach (DataRow row in oReportData.Rows) {
						ProcessTableReportRow(rptDef, new SafeReader(row), oTbody, lineCounter, oColumnTypes);
						lineCounter++;
					} // for each data row
				}
				catch (Exception e) {
					Alert(e, "Failed to create report table body.");
					return tbl;
				} // try
			} // if

			try {
				if (oColumnTypes != null) {
					for (int columnIndex = rptDef.Columns.Length - 1; columnIndex >= 0; columnIndex--)
						if (!rptDef.Columns[columnIndex].IsVisible)
							oColumnTypes.RemoveAt(columnIndex);
				} // if
			}
			catch (Exception e) {
				Alert(e, "Failed to finalise report table column types.");
			} // try

			return tbl;
		} // TableReport

		#endregion method TableReport

		#region method BuildTraficReport

		public ATag BuildTrafficReport(Report report, DateTime from, DateTime to, List<string> oColumnTypes = null) {
			var trafficReport = new TrafficReport(DB, this);
			KeyValuePair<ReportQuery, DataTable> oData = trafficReport.CreateTrafficReport(report, from, to);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(from, oToDate: to))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildTrafficReport

		#endregion method BuildTraficReport

		#region method BuildMarketingChannelsSummaryReport

		public ATag BuildMarketingChannelsSummaryReport(Report report, DateTime from, DateTime to, List<string> oColumnTypes = null) {
			var rpt = new MarketingChannelsSummary.MarketingChannelsSummary(DB, this);

			KeyValuePair<ReportQuery, DataTable> oData = rpt.Run(report, from, to);

			ATag oBody = new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(from, oToDate: to))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));

			if (from.Date.AddDays(1) == to.Date) {
				for (int i = 0; i < 6; i++) {
					from = from.AddDays(-1);
					to = to.AddDays(-1);

					oData = rpt.Run(report, from, to);

					oBody
						.Append(new H1().Append(new Text(report.GetTitle(from, oToDate: to))))
						.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
				} // for
			} // if

			return oBody;
		} // BuildMarketingChannelsSummaryReport

		#endregion method BuildMarketingChannelsSummaryReport

		#region method BuildLoanIntegrityReport

		public ATag BuildLoanIntegrityReport(Report report, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateLoanIntegrityReport(report);

			Body body = new Body();
			body.Add<Class>("Body");
			body.Append(new H1().Append(new Text("Loan integrity")));
			body.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));

			return body;
		} // BuildLoanIntegrityReport

		#endregion method BuildLoanIntegrityReport

		#region method BuildLoansIssuedReport

		public ATag BuildLoansIssuedReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> pair = CreateLoansIssuedReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(pair.Key, pair.Value, oColumnTypes: oColumnTypes)));
		} // BuildLoansIssuedReport

		#endregion method BuildLoansIssuedReport

		#region method BuildPlainedPaymentReport

		public ATag BuildPlainedPaymentReport(Report report, DateTime today, DateTime ignored, List<string> ignoredAgain = null) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(PaymentReport(today));
		} // BuildPlainedPaymentReport

		#endregion method BuildPlainedPaymentReport

		#region method BuildCciReport

		public ATag BuildCciReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateCciReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildCciReport

		#endregion method BuildCciReport

		#region method BuildUiReport

		public ATag BuildUiReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateUiReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildUiReport

		#endregion method BuildUiReport

		#region method BuildUiExtReport

		public ATag BuildUiExtReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateUiExtReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, isSharones: true, oColumnTypes: oColumnTypes)));
		} // BuildUiExtReport

		#endregion method BuildUiExtReport

		#region method BuildAccountingLoanBalanceReport

		public ATag BuildAccountingLoanBalanceReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateAccountingLoanBalanceReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildAccountingLoanBalanceReport

		#endregion method BuildAccountingLoanBalanceReport

		#endregion HTML generators

		#region Excel generators

		#region method BuildPlainedPaymentXls

		public ExcelPackage BuildPlainedPaymentXls(Report report, DateTime today, DateTime ignored) {
			var title = report.GetTitle(today);

			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(DateTime.Today.AddDays(3)))
				);

				return AddSheetToExcel(dt, title, "RptPaymentReport");
			}
			catch (Exception e) {
				Error(e.ToString());
				return new ExcelPackage();
			} // try
		} // BuildPlainedPaymentXls

		#endregion method BuildPlainedPaymentXls

		#region method BuildLoansIssuedXls

		public ExcelPackage BuildLoansIssuedXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> pair = CreateLoansIssuedReport(report, today, tomorrow);

			return AddSheetToExcel(pair.Value, report.GetTitle(today, oToDate: tomorrow), "RptLoansIssued");
		} // BuildLoansIssuedXls

		#endregion method BuildLoansIssuedXls

		#region method XlsReport

		public ExcelPackage XlsReport(ReportQuery rptDef, string sRptTitle = "") {
			try {
				return AddSheetToExcel(rptDef.Execute(DB), sRptTitle, rptDef.StoredProcedure, String.Empty);
			}
			catch (Exception e) {
				Alert(e, "Failed to generate Excel report.");
			} // try

			return ErrorXlsReport("Failed to generate report.");
		} // XlsReport

		public static ExcelPackage ErrorXlsReport(string sMsg) {
			var errBook = new ExcelPackage();
			var sheet = errBook.Workbook.Worksheets.Add("Error");
			sheet.Cells[1, 1].Value = "Error: " + sMsg;
			return errBook;
		} // ErrorXlsReport

		#endregion method XlsReport

		#region method BuildCciXls

		public ExcelPackage BuildCciXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateCciReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildCciXls

		#endregion method BuildCciXls

		#region method BuildUiXls

		public ExcelPackage BuildUiXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateUiReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptUiExt");
		} // BuildUiXls

		#endregion method BuildUiXls

		#region method BuildUiExtXls

		public ExcelPackage BuildUiExtXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateUiExtReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptUiExtReport");
		} // BuildUiExtXls

		#endregion method BuildUiExtXls

		#region method BuildAccountingLoanBalanceXls

		public ExcelPackage BuildAccountingLoanBalanceXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateAccountingLoanBalanceReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildAccountingLoanBalanceXls

		#endregion method BuildAccountingLoanBalanceXls

		#region method BuildTraficXls

		public ExcelPackage BuildTrafficReportXls(Report report, DateTime from, DateTime to) {
			var trafficReport = new TrafficReport(DB, this);
			KeyValuePair<ReportQuery, DataTable> oData = trafficReport.CreateTrafficReport(report, from, to);

			return AddSheetToExcel(oData.Value, report.GetTitle(from, oToDate: to), report.Title);
		} // BuildTrafficReportXls

		#endregion method BuildTraficXls

		#region method BuildMarketingChannelsSummaryXls

		public ExcelPackage BuildMarketingChannelsSummaryXls(Report report, DateTime from, DateTime to) {
			var rpt = new MarketingChannelsSummary.MarketingChannelsSummary(DB, this);
			KeyValuePair<ReportQuery, DataTable> oData = rpt.Run(report, from, to);

			ExcelPackage wb = AddSheetToExcel(oData.Value, report.GetTitle(from, oToDate: to), report.Title);

			if (from.Date.AddDays(1) == to.Date) {
				for (int i = 1; i < 7; i++) {
					from = from.AddDays(-1);
					to = to.AddDays(-1);

					oData = rpt.Run(report, from, to);

					wb = AddSheetToExcel(oData.Value, report.GetTitle(from, oToDate: to), i + " day" + (i == 1 ? "" : "s") + " before", report.Title, wb: wb);
				} // for
			} // if

			return wb;
		} // BuildMarketingChannelsSummarXls

		#endregion method BuildMarketingChannelsSummaryXls

		#endregion Excel generators

		#endregion public

		#region protected

		protected AConnection DB { get; private set; }

		#endregion protected

		#region private

		#region property UnderwriterSite

		private string UnderwriterSite {
			get {
				if (string.IsNullOrWhiteSpace(m_sUnderwriterSite)) {
					DB.ForEachRowSafe(
						(sr, bRowsetStart) => {
							m_sUnderwriterSite = sr["Value"];
							return ActionResult.SkipAll;
						},
						"LoadConfigurationVariable",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@CfgVarName", "UnderwriterSite")
					);
				} // if

				return m_sUnderwriterSite;
			} // get
		} // UnderwriterSite

		private string m_sUnderwriterSite;

		#endregion property UnderwriterSite

		#region report generators

		#region LoanIntegrity

		#region class LoanIntegrityRow

		private class LoanIntegrityRow {
			[UsedImplicitly]
			public int LoanId { get; set; }

			[UsedImplicitly]
			public decimal Diff { get; set; }

			public static int Compare(LoanIntegrityRow a, LoanIntegrityRow b) {
				if (a.Diff == b.Diff)
					return 0;
				if (a.Diff > b.Diff)
					return 1;
				return -1;
			} // Compare

			public void ToRow(DataTable tbl) {
				DataRow row = tbl.NewRow();
				row["LoanId"] = LoanId;
				row["Diff"] = Diff;
				tbl.Rows.Add(row);
			} // ToRow
		} // class LoanIntegrityRow

		#endregion class LoanIntegrityRow

		#region method CreateLoanIntegrityReport

		private KeyValuePair<ReportQuery, DataTable> CreateLoanIntegrityReport(Report report) {
			var loanIntegrity = new LoanIntegrity(DB, this);
			SortedDictionary<int, decimal> diffs = loanIntegrity.Run();

			var oRows = new List<LoanIntegrityRow>();

			foreach (int loanId in diffs.Keys) {
				var oNewRow = new LoanIntegrityRow {
					LoanId = loanId,
					Diff = diffs[loanId]
				};

				oRows.Add(oNewRow);
			} // foreach

			oRows.Sort(LoanIntegrityRow.Compare);

			var oOutput = new DataTable();

			oOutput.Columns.Add("LoanID", typeof(int));
			oOutput.Columns.Add("Diff", typeof(decimal));

			oRows.ForEach(r => r.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(new ReportQuery(report), oOutput); // qqq - the new repoerQuery here is wrong
		} // CreateLoanIntegrityReport

		#endregion method CreateLoanIntegrityReport

		#endregion LoanIntegrity

		#region Loans Issued

		#region class LoansIssuedRow

		private class LoansIssuedRow {
			#region public

			#region constructor

			public LoansIssuedRow(SafeReader row) {
				m_oData = new SortedDictionary<string, dynamic>();

				if (row == null) {
					m_bIsTotal = true;

					m_oClients = new SortedDictionary<int, int>();

					foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames)
						m_oData[pair.Key] = pair.Value;

					m_oData[FldClientName] = "Total";
					m_oData[FldRowLevel] = "total";
				}
				else {
					m_bIsTotal = false;

					foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames)
						m_oData[pair.Key] = row.ColumnOrDefault(pair.Key);
				} // if
			} // constructor

			#endregion constructor

			#region method SetInterests

			public void SetInterests(SortedDictionary<int, decimal> oEarnedInterest) {
				if (oEarnedInterest.ContainsKey(LoanID))
					m_oData[FldEarnedInterest] = oEarnedInterest[LoanID];

				m_oData[FldAccruedInterest] = (decimal)m_oData[FldEarnedInterest] - (decimal)m_oData[FldTotalInterestRepaid];

				// Until now the field ExpectedInterest only contains sum of planned
				// interest from LoanSchedule. Accrued should be subtructed from it.
				m_oData[FldExpectedInterest] = (decimal)m_oData[FldExpectedInterest] - (decimal)m_oData[FldAccruedInterest];

				m_oData[FldTotalInterest] = (decimal)m_oData[FldExpectedInterest] + (decimal)m_oData[FldEarnedInterest];

				foreach (string sIdx in new string[] { FldAccruedInterest, FldExpectedInterest, FldTotalInterest }) {
					decimal x = m_oData[sIdx];

					if (Math.Abs(x) < 0.01M)
						m_oData[sIdx] = 0M;
				} // for each
			} // SetInterests

			#endregion method SetInterests

			#region method AccumulateTotals

			public void AccumulateTotals(LoansIssuedRow row) {
				foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames) {
					if (ms_oTotalIgnored.ContainsKey(pair.Key))
						continue;

					m_oData[pair.Key] += row.m_oData[pair.Key];
				} // for each field name
			} // AccumulateTotals

			#endregion method AccumulateTotals

			#region method SetLoanCount

			public void SetLoanCount(int nLoanCount) {
				m_oData[FldLoanID] = nLoanCount;
			} // SetLoanCount

			#endregion method SetLoanCount

			#region method AddClient

			public void AddClient(LoansIssuedRow lir) {
				if (m_oClients.ContainsKey(lir.ClientID))
					m_oClients[lir.ClientID]++;
				else
					m_oClients[lir.ClientID] = 1;
			} // AddClient

			#endregion method AddClient

			#region method ToTable

			public DataTable ToTable() {
				var tbl = new DataTable();

				foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames)
					tbl.Columns.Add(pair.Key, FieldType(pair.Key));

				ToRow(tbl);

				return tbl;
			} // ToTable

			#endregion method ToTable

			#region method ToRow

			public void ToRow(DataTable tbl) {
				DataRow row = tbl.NewRow();

				foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames) {
					string sFldName = pair.Key;

					dynamic oValue = m_oData[sFldName];

					if (m_bIsTotal) {
						switch (sFldName) {
						case FldPeriod:
						case FldBaseInterest:
							oValue = (LoanID == 0) ? oValue : oValue / (decimal)LoanID;
							break;

						case FldClientID:
							oValue = m_oClients.Count;
							break;
						} // switch
					} // if

					row[sFldName] = oValue;
				} // foreach

				tbl.Rows.Add(row);
			} // ToRow

			#endregion method ToRow

			#endregion public

			#region private

			private readonly SortedDictionary<string, dynamic> m_oData;
			private readonly bool m_bIsTotal;

			private readonly SortedDictionary<int, int> m_oClients;

			#region property ClientID

			private int ClientID { get { return m_oData[FldClientID]; } } // ClientID

			#endregion property ClientID

			#region property LoanID

			private int LoanID { get { return m_oData[FldLoanID]; } } // LoanID

			#endregion property LoanID

			#region private static

			#region field name constants

			private const string FldLoanID = "LoanID";
			private const string FldDate = "Date";
			private const string FldClientID = "ClientID";
			private const string FldClientEmail = "ClientEmail";
			private const string FldClientName = "ClientName";
			private const string FldLoanTypeName = "LoanTypeName";
			private const string FldEU = "EU";
			private const string FldSetupFee = "SetupFee";
			private const string FldLoanAmount = "LoanAmount";
			private const string FldPeriod = "Period";
			private const string FldPlannedInterest = "PlannedInterest";
			private const string FldPlannedRepaid = "PlannedRepaid";
			private const string FldTotalPrincipalRepaid = "TotalPrincipalRepaid";
			private const string FldTotalInterestRepaid = "TotalInterestRepaid";
			private const string FldEarnedInterest = "EarnedInterest";
			private const string FldExpectedInterest = "ExpectedInterest";
			private const string FldAccruedInterest = "AccruedInterest";
			private const string FldTotalInterest = "TotalInterest";
			private const string FldTotalFeesRepaid = "TotalFeesRepaid";
			private const string FldTotalCharges = "TotalCharges";
			private const string FldBaseInterest = "BaseInterest";
			private const string FldDiscountPlan = "DiscountPlan";
			private const string FldCustomerStatus = "CustomerStatus";
			private const string FldRowLevel = "RowLevel";

			#endregion field name constants

			#region static constructor

			static LoansIssuedRow() {
				ms_oFieldNames = new SortedDictionary<string, dynamic>();
				ms_oFieldNames[FldLoanID] = 0;
				ms_oFieldNames[FldDate] = DBNull.Value;
				ms_oFieldNames[FldClientID] = 0;
				ms_oFieldNames[FldClientEmail] = "";
				ms_oFieldNames[FldClientName] = "";
				ms_oFieldNames[FldLoanTypeName] = "";
				ms_oFieldNames[FldEU] = "";
				ms_oFieldNames[FldSetupFee] = 0;
				ms_oFieldNames[FldLoanAmount] = 0;
				ms_oFieldNames[FldPeriod] = 0;
				ms_oFieldNames[FldPlannedInterest] = 0;
				ms_oFieldNames[FldPlannedRepaid] = 0;
				ms_oFieldNames[FldTotalPrincipalRepaid] = 0;
				ms_oFieldNames[FldTotalInterestRepaid] = 0;
				ms_oFieldNames[FldEarnedInterest] = 0;
				ms_oFieldNames[FldExpectedInterest] = 0;
				ms_oFieldNames[FldAccruedInterest] = 0;
				ms_oFieldNames[FldTotalInterest] = 0;
				ms_oFieldNames[FldTotalFeesRepaid] = 0;
				ms_oFieldNames[FldTotalCharges] = 0;
				ms_oFieldNames[FldBaseInterest] = 0;
				ms_oFieldNames[FldDiscountPlan] = "";
				ms_oFieldNames[FldCustomerStatus] = "";
				ms_oFieldNames[FldRowLevel] = "";

				ms_oTotalIgnored = new SortedDictionary<string, int>();
				ms_oTotalIgnored[FldLoanID] = 0;
				ms_oTotalIgnored[FldDate] = 0;
				ms_oTotalIgnored[FldClientID] = 0;
				ms_oTotalIgnored[FldClientEmail] = 0;
				ms_oTotalIgnored[FldClientName] = 0;
				ms_oTotalIgnored[FldLoanTypeName] = 0;
				ms_oTotalIgnored[FldEU] = 0;
				ms_oTotalIgnored[FldDiscountPlan] = 0;
				ms_oTotalIgnored[FldCustomerStatus] = 0;
				ms_oTotalIgnored[FldRowLevel] = 0;
			} // static constructor

			#endregion static constructor

			#region method FieldType

			private static Type FieldType(string sFldName) {
				var oFldType = typeof(decimal);

				if (ms_oTotalIgnored.ContainsKey(sFldName)) {
					switch (sFldName) {
					case FldLoanID:
					case FldClientID:
						oFldType = typeof(int);
						break;

					case FldDate:
						oFldType = typeof(DateTime);
						break;

					default:
						oFldType = typeof(string);
						break;
					} // switch
				} // if

				return oFldType;
			} // FieldType

			#endregion method FieldType

			private static readonly SortedDictionary<string, int> ms_oTotalIgnored;
			private static readonly SortedDictionary<string, dynamic> ms_oFieldNames;

			#endregion private static

			#endregion private
		} // class LoansIssuedRow

		#endregion class LoansIssuedRow

		#region method CreateLoansIssuedReport

		private KeyValuePair<ReportQuery, DataTable> CreateLoansIssuedReport(Report report, DateTime today, DateTime tomorrow) {
			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.ByIssuedLoans, false, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			var oRows = new List<LoansIssuedRow>();

			var oTotal = new LoansIssuedRow(null);

			DataTable tbl = rpt.Execute(DB);

			rpt.Execute(DB, (sr, bRowsetStart) => {
				var lir = new LoansIssuedRow(sr);
				oRows.Add(lir);

				lir.SetInterests(earned);

				oTotal.AddClient(lir);
				oTotal.AccumulateTotals(lir);

				return ActionResult.Continue;
			});

			oTotal.SetLoanCount(tbl.Rows.Count);

			DataTable oOutput = oTotal.ToTable();

			oRows.ForEach(lir => lir.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateLoansIssuedReport

		#endregion method CreateLoansIssuedReport

		#endregion Loans Issued

		#region method PaymentReport

		private ATag PaymentReport(DateTime today) {
			Table tbl = new Table();

			try {
				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Id")))
					.Append(new Th().Append(new Text("Name")))
					.Append(new Th().Append(new Text("Email")))
					.Append(new Th().Append(new Text("Date")))
					.Append(new Th().Append(new Text("Amount")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<ID>("tableReportData").Add<Class>("Report").Append(new Thead().Append(oTr));

				var tbody = new Tbody();
				tbl.Append(tbody);

				DB.ForEachRowSafe(
					(sr, bRowsetStart) => {
						oTr = new Tr()
							.Append(new Td().Add<Class>("L").Append(new Text(sr["Id"])))
							.Append(new Td().Add<Class>("L").Append(new Text(sr["Firstname"] + " " + sr["SurName"])))
							.Append(new Td().Add<Class>("L").Append(new Text(sr["Name"])))
							.Append(new Td().Add<Class>("L").Append(new Text(sr["DATE"])))
							.Append(new Td().Add<Class>("R").Append(new Text(sr["AmountDue"])));

						tbody.Append(oTr);

						return ActionResult.Continue;
					},
					"RptPaymentReport",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(DateTime.Today.AddDays(3)))
				);
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // PaymentReport

		#endregion method PaymentReport

		#region method CreateCciReport

		private KeyValuePair<ReportQuery, DataTable> CreateCciReport(Report report, DateTime today, DateTime tomorrow) {
			var cc = new CciReport(DB, this);
			List<CciReportItem> oItems = cc.Run();

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			DataTable oOutput = CciReportItem.CreateTable();

			oItems.ForEach(r => r.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateCciReport

		#endregion method CreateCciReport

		#region method CreateUiReport

		private KeyValuePair<ReportQuery, DataTable> CreateUiReport(Report report, DateTime today, DateTime tomorrow) {
			var cc = new UiReport(DB, today, tomorrow, this);
			List<UiReportItem> oItems = cc.Run();

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			DataTable oOutput = UiReportItem.CreateTable();

			foreach (UiReportItem oItem in oItems)
				oItem.ToRow(oOutput);

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateUiReport

		#endregion method CreateUiReport

		#region method CreateUiExtReport

		private KeyValuePair<ReportQuery, DataTable> CreateUiExtReport(Report report, DateTime today, DateTime tomorrow) {
			var cc = new UiReportExt(DB, today, tomorrow, this);

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			Tuple<DataTable, ColumnInfo[]> oOutput = cc.Run();

			rpt.Columns = oOutput.Item2;

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput.Item1);
		} // CreateUiExtReport

		#endregion method CreateUiExtReport

		#region Accounting Loan Balance

		#region class AccountingLoanBalanceRawData

		public class AccountingLoanBalanceRawData {
			public DateTime IssueDate { get; set; }
			public int ClientID { get; set; }
			public int LoanID { get; set; }
			public string ClientName { get; set; }
			public string ClientEmail { get; set; }
			public decimal IssuedAmount { get; set; }
			public decimal SetupFee { get; set; }
			public decimal FeesEarned { get; set; }
			public string LoanStatus { get; set; }
			public string LoanTranMethod { get; set; }
			public decimal TotalRepaid { get; set; }
			public decimal FeesRepaid { get; set; }
			public decimal RolloverRepaid { get; set; }
			public int FeesEarnedID { get; set; }
			public int TransactionID { get; set; }
			public string CustomerStatus { get; set; }
		} // AccountingLoanBalanceRawData

		#endregion class AccountingLoanBalanceRawData

		#region class AccountingLoanBalanceRow

		private class AccountingLoanBalanceRow {
			#region public

			#region fields

			[UsedImplicitly]
			public DateTime IssueDate;
			[UsedImplicitly]
			public int ClientID;
			[UsedImplicitly]
			public int LoanID;
			[UsedImplicitly]
			public string ClientName;
			[UsedImplicitly]
			public string ClientEmail;
			[UsedImplicitly]
			public decimal IssuedAmount;
			[UsedImplicitly]
			public decimal SetupFee;
			[UsedImplicitly]
			public string LoanStatus;
			[UsedImplicitly]
			public decimal EarnedInterest;
			[UsedImplicitly]
			public decimal EarnedFees;
			[UsedImplicitly]
			public decimal CashPaid;
			[UsedImplicitly]
			public string CustomerStatus;

			#endregion fields

			#region constructor

			public AccountingLoanBalanceRow(SafeReader sr, decimal nEarnedInterest) {
				Transactions = new SortedSet<int>();
				LoanCharges = new SortedSet<int>();

				AccountingLoanBalanceRawData raw = sr.Fill<AccountingLoanBalanceRawData>();

				IssueDate = raw.IssueDate;
				ClientID = raw.ClientID;
				LoanID = raw.LoanID;
				ClientName = raw.ClientName;
				ClientEmail = raw.ClientEmail;
				IssuedAmount = raw.IssuedAmount;
				SetupFee = raw.SetupFee;
				EarnedFees = raw.FeesEarned;
				LoanStatus = raw.LoanStatus;
				CustomerStatus = raw.CustomerStatus;
				EarnedInterest = nEarnedInterest;
				CashPaid = 0;

				if (SetupFee > 0)
					IssuedAmount -= SetupFee;

				Update(raw.TransactionID, raw.LoanTranMethod, raw.TotalRepaid, raw.RolloverRepaid, raw.FeesEarnedID);
			} // constructor

			#endregion constructor

			#region method Update

			public void Update(SafeReader sr) {
				Update(sr["TransactionID"], sr["LoanTranMethod"], sr["TotalRepaid"], sr["RolloverRepaid"], sr["FeesEarnedID"]);
			} // Update

			private void Update(int nTransactionID, string sMethod, decimal nAmount, decimal nRolloverRepaid, int nFeesEarnedID) {
				if (!LoanCharges.Contains(nFeesEarnedID))
					LoanCharges.Add(nFeesEarnedID);

				if (!Transactions.Contains(nTransactionID)) {
					Transactions.Add(nTransactionID);

					sMethod = sMethod ?? string.Empty;
					if (!sMethod.ToLower().StartsWith("non-cash"))
						CashPaid += nAmount;

					EarnedFees += nRolloverRepaid;
				} // if
			} // Update

			#endregion method Update

			#region method ToRow

			public void ToRow(DataTable tbl) {
				tbl.Rows.Add(
					IssueDate, ClientID, LoanID, ClientName, ClientEmail, LoanStatus,
					IssuedAmount, SetupFee, EarnedInterest, EarnedFees,
					CashPaid, Balance, CustomerStatus
				);
			} // ToRow

			#endregion method ToRow

			#region method ToTable

			public static DataTable ToTable() {
				var oOutput = new DataTable();

				oOutput.Columns.Add("IssueDate", typeof(DateTime));
				oOutput.Columns.Add("ClientID", typeof(int));
				oOutput.Columns.Add("LoanID", typeof(int));
				oOutput.Columns.Add("ClientName", typeof(string));
				oOutput.Columns.Add("ClientEmail", typeof(string));
				oOutput.Columns.Add("LoanStatus", typeof(string));
				oOutput.Columns.Add("IssuedAmount", typeof(decimal));
				oOutput.Columns.Add("SetupFee", typeof(decimal));
				oOutput.Columns.Add("EarnedInterest", typeof(decimal));
				oOutput.Columns.Add("EarnedFees", typeof(decimal));
				oOutput.Columns.Add("CashPaid", typeof(decimal));
				oOutput.Columns.Add("Balance", typeof(decimal));
				oOutput.Columns.Add("CustomerStatus", typeof(string));

				return oOutput;
			} // ToTable

			#endregion method ToTable

			#endregion public

			#region private

			#region property Balance

			private decimal Balance {
				get { return IssuedAmount + SetupFee + EarnedInterest + EarnedFees - CashPaid; } // get
			} // Balance

			#endregion property Balance

			#region property Transactions

			private SortedSet<int> Transactions { get; set; }

			#endregion property Transactions

			#region property LoanCharges

			private SortedSet<int> LoanCharges { get; set; }

			#endregion property LoanCharges

			#endregion private
		} // class AccountingLoanBalanceRow

		#endregion class AccountingLoanBalanceRow

		#region method CreateAccountingLoanBalanceReport

		private KeyValuePair<ReportQuery, DataTable> CreateAccountingLoanBalanceReport(Report report, DateTime today, DateTime tomorrow) {
			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.AccountingLoanBalance, false, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var oRows = new SortedDictionary<int, AccountingLoanBalanceRow>();

			rpt.Execute(DB, (sr, bRowsetStart) => {
				int nLoanID = sr["LoanID"];

				decimal nEarnedInterest = earned.ContainsKey(nLoanID) ? earned[nLoanID] : 0;

				if (oRows.ContainsKey(nLoanID))
					oRows[nLoanID].Update(sr);
				else
					oRows[nLoanID] = new AccountingLoanBalanceRow(sr, nEarnedInterest);

				return ActionResult.Continue;
			});

			DataTable oOutput = AccountingLoanBalanceRow.ToTable();

			foreach (KeyValuePair<int, AccountingLoanBalanceRow> pair in oRows)
				pair.Value.ToRow(oOutput);

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateAccountingLoanBalanceReport

		#endregion method CreateAccountingLoanBalanceReport

		#endregion Accounting Loan Balance

		#endregion report generators

		#region method ProcessTableReportRow

		private void ProcessTableReportRow(ReportQuery rptDef, SafeReader sr, Tbody oTbody, int lineCounter, List<string> oColumnTypes) {
			var oTr = new Tr().Add<Class>(lineCounter % 2 == 0 ? "Odd" : "Even");
			oTbody.Append(oTr);

			var oClassesToApply = new List<string>();

			for (int columnIndex = 0; columnIndex < rptDef.Columns.Length; columnIndex++) {
				ColumnInfo col = rptDef.Columns[columnIndex];
				var oValue = sr.ColumnOrDefault(col.FieldName);

				if (col.IsVisible) {
					var oTd = new Td();
					oTr.Append(oTd);

					if (IsNumber(oValue)) {
						ATag oInnerTag = new Text(NumStr(oValue, col.Format(IsInt(oValue) ? 0 : 2)));

						if (col.ValueType == ValueType.UserID || col.ValueType == ValueType.BrokerID) {
							var oLink = new A();

							oLink.Append(oInnerTag);
							oLink.Target.Append("_blank");

							var titleText = "Open this customer in underwriter.";
							if (col.ValueType == ValueType.UserID) {
								oLink.Href.Append("https://" + UnderwriterSite + "/UnderWriter/Customers?customerid=" + oValue);
							}
							else {
								oLink.Href.Append("https://" + UnderwriterSite + "/UnderWriter/Customers#broker/" + oValue);
								titleText = "Open this broker in underwriter.";
							}

							oLink.Alt.Append(titleText);
							oLink.Title.Append(titleText);

							oInnerTag = oLink;

							if (oColumnTypes != null)
								oColumnTypes[columnIndex] = "user-id";
						}
						else {
							if (oColumnTypes != null)
								oColumnTypes[columnIndex] = "formatted-num";
						} // if user id

						oTd.Add<Class>("R").Append(oInnerTag);
					}
					else {
						oTd.Add<Class>("L").Append(new Text(oValue.ToString()));

						if ((oColumnTypes != null) && (oValue is DateTime))
							oColumnTypes[columnIndex] = "date";
					} // if
				}
				else {
					if (col.ValueType == ValueType.CssClass)
						oClassesToApply.Add(oValue.ToString());
				} // if
			} // for each column

			if (oClassesToApply.Count > 0)
				oTr.ApplyToChildren<Class>(string.Join(" ", oClassesToApply.ToArray()));
		} // ProcessTableReportRow

		#endregion method ProcessTableReportRow

		#region private static

		#region method IsNumber

		private static bool IsNumber(object value) {
			return IsInt(value) || IsFloat(value);
		} // IsNumber

		#endregion method IsNumber

		#region method IsInt

		private static bool IsInt(object value) {
			return value is sbyte
				|| value is byte
				|| value is short
				|| value is ushort
				|| value is int
				|| value is uint
				|| value is long
				|| value is ulong;
		} // IsInt

		#endregion method IsInt

		#region method IsFloat

		private static bool IsFloat(object value) {
			return value is float
				|| value is double
				|| value is decimal;
		} // IsFloat

		#endregion method IsFloat

		#region method NumStr

		private static string NumStr(object oNumber, string sFormat) {
			if (oNumber is sbyte)
				return ((sbyte)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is byte)
				return ((byte)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is short)
				return ((short)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is ushort)
				return ((ushort)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is int)
				return ((int)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is uint)
				return ((uint)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is long)
				return ((long)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is ulong)
				return ((ulong)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is float)
				return ((float)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is double)
				return ((double)oNumber).ToString(sFormat, ms_oFormatInfo);
			if (oNumber is decimal)
				return ((decimal)oNumber).ToString(sFormat, ms_oFormatInfo);

			throw new Exception(string.Format("Unsupported type: {0}", oNumber.GetType()));
		} // NumStr

		#endregion method NumStr

		#region method AddSheetToExcel

		private static ExcelPackage AddSheetToExcel(DataTable dt, string title, string sheetName = "", string someText = "", ExcelPackage wb = null) {
			if (wb == null) // first initialization, if we will use it multiple times.
				wb = new ExcelPackage();

			if (string.IsNullOrEmpty(sheetName))
				sheetName = "Report"; // default sheetName

			var sheet = wb.Workbook.Worksheets.Add(sheetName);

			const int nTitleRow = 1; // first row for title
			const int nFirstColumn = 1; // first column

			int nHeaderRow = nTitleRow + 1;

			if (!string.IsNullOrWhiteSpace(someText)) {
				sheet.Cells[nTitleRow + 1, nFirstColumn].Value = someText;
				nHeaderRow++;
			} // if

			int nFirstDataRow = nHeaderRow + 1;

			int nRowCount = dt.Rows.Count;
			int nColumnCount = dt.Columns.Count;

			if (nColumnCount > 1)
				sheet.Cells[nTitleRow, nFirstColumn, nTitleRow, nFirstColumn + nColumnCount - 1].Merge = true;

			ExcelRange oTitle = sheet.Cells[nTitleRow, nFirstColumn];
			oTitle.Value = title.Replace("<h1>", "").Replace("</h1>", "");
			oTitle.Style.Font.Bold = true;
			oTitle.Style.Font.Size = 16;

			for (int i = 0; i < nRowCount; i++) {
				DataRow row = dt.Rows[i];

				for (int j = 0; j < nColumnCount; j++) {
					ExcelRange oCell = sheet.Cells[nFirstDataRow + i, nFirstColumn + j];
					var oValue = row[j];

					if (IsInt(oValue))
						oCell.Style.Numberformat.Format = "#,##0";
					else if (IsFloat(oValue))
						oCell.Style.Numberformat.Format = "#,##0.00";
					else if (oValue is DateTime)
						oCell.Style.Numberformat.Format = "dd-mmm-yyyy hh:mm:ss";
					else
						oCell.Style.Numberformat.Format = "@";

					oCell.Value = oCell.Style.Numberformat.Format == "@"
						? oValue.ToString().Replace("&nbsp;", " ")
						: oValue;
				} // for each column
			} // for each row

			for (int i = 0; i < nColumnCount; i++)
				sheet.Cells[nHeaderRow, nFirstColumn + i].Value = dt.Columns[i].Caption;

			sheet.Cells.AutoFitColumns();
			sheet.Row(nHeaderRow).Style.Font.Bold = true;

			return wb;
		} // AddSheetToExcel

		#endregion method AddSheetToExcel

		private static readonly CultureInfo ms_oFormatInfo = new CultureInfo("en-GB");

		#endregion private static

		#endregion private
	} // class BaseReportHandler
} // namespace Reports
