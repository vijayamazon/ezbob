using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using Ezbob.Database;
using Ezbob.Logger;
using Aspose.Cells;
using Html;
using Html.Attributes;
using Html.Tags;

namespace Reports {
	public class BaseReportHandler : SafeLog {
		#region public

		#region method InitAspose

		public static void InitAspose() {
			var license = new License();

			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Reports.Aspose.Total.lic")) {
				s.Position = 0;
				license.SetLicense(s);
			} // using
		} // InitAspose

		#endregion method InitAspose

		#region constructor

		public BaseReportHandler(AConnection oDB, ASafeLog log = null) : base(log) {
			DB = oDB;
		} // constructor

		#endregion constructor

		#region method GetReportList

		public SortedDictionary<string, Report> GetReportsList(string userName = null) {
			var users = new List<QueryParameter>();

			userName = (userName ?? "").Trim();

			if (userName != string.Empty)
				users.Add(new QueryParameter("@UserName", userName));

			DataTable dt = DB.ExecuteReader("RptGetUserReports", users.ToArray());

			var reportList = new SortedDictionary<string, Report>();

			foreach (DataRow row in dt.Rows) {
				var rpt = new Report(row, BaseReportSender.DefaultToEMail);

				reportList[rpt.TypeName] = rpt;
			} // for each

			DataTable args = Report.LoadReportArgs(DB);

			Report oLastReport = null;
			string sLastType = null;

			foreach (DataRow row in args.Rows) {
				string sTypeName = row["ReportType"].ToString();
				string sArgName = row["ArgumentName"].ToString();

				if (sLastType != sTypeName) {
					if (reportList.ContainsKey(sTypeName)) {
						sLastType = sTypeName;
						oLastReport = reportList[sLastType];
					}
					else
						continue;
				} // if

				oLastReport.AddArgument(sArgName);
			} // for each

			return reportList;
		} // GetReportsList

		#endregion method GetReportList

		#region HTML generators

		#region method TableReport

		public ATag TableReport(ReportQuery rptDef, bool isSharones = false, string sRptTitle = "", List<string> oColumnTypes = null) {
			return TableReport(rptDef, null, isSharones, sRptTitle, oColumnTypes);
		} // TableReport

		public ATag TableReport(ReportQuery rptDef, DataTable oReportData, bool isSharones = false, string sRptTitle = "", List<string> oColumnTypes = null) {
			var tbl = new Table().Add<Class>("Report");

			try {
				DataTable dt = oReportData ?? rptDef.Execute(DB);

				if (!isSharones)
					tbl.Add<ID>("tableReportData");

				var tr = new Tr().Add<Class>("HR");

				for (int columnIndex = 0; columnIndex < rptDef.Columns.Length; columnIndex++)
					if (rptDef.Columns[columnIndex].IsVisible)
						tr.Append(new Th().Add<Class>("H").Append(new Text(rptDef.Columns[columnIndex].Caption)));

				tbl.Append(new Thead().Append(tr));

				var oTbody = new Tbody();
				tbl.Append(oTbody);

				int lineCounter = 0;

				if (oColumnTypes != null) {
					oColumnTypes.Clear();

					for (int columnIndex = 0; columnIndex < rptDef.Columns.Length; columnIndex++)
						oColumnTypes.Add("string");
				} // if

				foreach (DataRow row in dt.Rows) {
					var oTr = new Tr().Add<Class>(lineCounter % 2 == 0 ? "Odd" : "Even");
					oTbody.Append(oTr);

					List<string> oClassesToApply = new List<string>();

					for (int columnIndex = 0; columnIndex < rptDef.Columns.Length; columnIndex++) {
						ColumnInfo col = rptDef.Columns[columnIndex];
						var oValue = row[col.FieldName];

						if (col.IsVisible) {
							var oTd = new Td();
							oTr.Append(oTd);

							if (IsNumber(oValue)) {
								oTd.Add<Class>("R").Append(new Text(NumStr(oValue, col.Format(IsInt(oValue) ? 0 : 2))));

								if (oColumnTypes != null)
									oColumnTypes[columnIndex] = "formatted-num";
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

					lineCounter++;
				} // for each data row

				AddSheetToExcel(dt, rptDef.StoredProcedure, sRptTitle);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			if (oColumnTypes != null) {
				for (int columnIndex = rptDef.Columns.Length - 1; columnIndex >= 0; columnIndex--)
					if (!rptDef.Columns[columnIndex].IsVisible)
						oColumnTypes.RemoveAt(columnIndex);
			} // if

			return tbl;
		} // TableReport

		#endregion method TableReport

		#region method BuildNewClientReport

		public ATag BuildNewClientReport(Report report, DateTime today) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(AdsReport(today))
				.Append(CustomerReport(today));
		} // BuildNewClientReport

		#endregion method BuildNewClientReport

		#region method BuildInWizardReport

		public ATag BuildInWizardReport(Report report, DateTime today, DateTime tomorrow) {
			var rptNewClients = new ReportQuery(report) {
				StoredProcedure = "RptNewClients",
				DateStart = today,
				DateEnd = tomorrow,
				Columns = GetHeaderAndFields(ReportType.RPT_IN_WIZARD)
			};

			var rptNewClientsStep1 = new ReportQuery(report) {
				StoredProcedure = "RptNewClientsStep1",
				DateStart = today,
				DateEnd = tomorrow,
				Columns = rptNewClients.Columns
			};

			return new Html.Tags.Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(new P().Append(new Text("Clients that enetered Shops but did not complete:")))

				.Append(new P().Append(TableReport(rptNewClients, true)))

				.Append(new P().Append(new Text("Clients that just enetered their email:")))

				.Append(new P().Append(TableReport(rptNewClientsStep1, true)));
		} // BuildInWizardReport

		#endregion method BuildInWizardReport

		#region method BuildEarnedInterestReport

		public ATag BuildEarnedInterestReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateEarnedInterestReport(report, today, tomorrow);

			return new Html.Tags.Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildEarnedInterestReport

		#endregion method BuildEarnedInterestReport


		public ATag BuildLoanIntegrityReport(Report report, List<string> oColumnTypes = null)
		{
			KeyValuePair<ReportQuery, DataTable> oData = CreateLoanIntegrityReport(report);

			Body body = new Body();
			body.Add<Class>("Body");
			body.Append(new H1().Append(new Text("Loan integrity")));
			body.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));

			return body;
		}

		#region method BuildLoansIssuedReport

		public ATag BuildLoansIssuedReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> pair = CreateLoansIssuedReport(report, today, tomorrow);

			return new Html.Tags.Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(pair.Key, pair.Value, oColumnTypes: oColumnTypes)));
		} // BuildLoansIssuedReport

		#endregion method BuildLoansIssuedReport

		#region method BuildPlainedPaymentReport

		public ATag BuildPlainedPaymentReport(Report report, DateTime today) {
			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today))))
				.Append(PaymentReport(today));
		} // BuildPlainedPaymentReport

		#endregion method BuildPlainedPaymentReport

		#endregion HTML generators

		#region Excel generators

		#region method BuildNewClientXls

		public Workbook BuildNewClientXls(Report report, DateTime today) {
			var title = report.GetTitle(today);
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", DB.DateToString(today)));
				wb = AddSheetToExcel(dt, title, "RptAdsReport");
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			try {
				DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", DB.DateToString(today)));
				wb = AddSheetToExcel(dt, title, "RptCustomerReport", String.Empty, wb);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildNewClientXls

		#endregion method BuildNewClientXls

		#region method BuildPlainedPaymentXls

		public Workbook BuildPlainedPaymentXls(Report report, DateTime today) {
			var title = report.GetTitle(today);
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(DateTime.Today.AddDays(3)))
				);

				wb = AddSheetToExcel(dt, title, "RptPaymentReport");
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildPlainedPaymentXls

		#endregion method BuildPlainedPaymentXls

		#region method BuildInWizardXls

		public Workbook BuildInWizardXls(Report report, DateTime today, DateTime tomorrow) {
			var title = report.GetTitle(today);
			var sometext = String.Empty;
			var wb = new Workbook();

			try {
				DataTable dt = DB.ExecuteReader("RptNewClients",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(tomorrow))
				);

				sometext = "Clients that entered Shops but did not complete:";
				wb = AddSheetToExcel(dt, title, "RptNewClients", sometext);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			try {
				DataTable dt = DB.ExecuteReader("RptNewClientsStep1",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(tomorrow))
				);

				sometext = "Clients that just entered their email:";
				wb = AddSheetToExcel(dt, title, "RptNewClientsStep1", sometext, wb);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // BuildInWizardXls

		#endregion method BuildInWizardXls

		#region method BuildEarnedInterestXls

		public Workbook BuildEarnedInterestXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateEarnedInterestReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildEarnedInterestXls

		#endregion method BuildEarnedInterestXls

		#region method BuildLoansIssuedXls

		public Workbook BuildLoansIssuedXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> pair = CreateLoansIssuedReport(report, today, tomorrow);

			return AddSheetToExcel(pair.Value, report.GetTitle(today, oToDate: tomorrow), "RptLoansIssued");
		} // BuildLoansIssuedXls

		#endregion method BuildLoansIssuedXls

		#region method XlsReport

		public Workbook XlsReport(ReportQuery rptDef, string sRptTitle = "") {
			var wb = new Workbook();

			try {
				DataTable dt = rptDef.Execute(DB);
				wb = AddSheetToExcel(dt, sRptTitle, rptDef.StoredProcedure, String.Empty);
			}
			catch (Exception e) {
				Error(e.ToString());
			} // try

			return wb;
		} // XlsReport

		#endregion method XlsReport

		#endregion Excel generators

		#endregion public

		#region protected

		protected AConnection DB { get; private set; }

		#endregion protected

		#region private

		#region method GetHeaderAndFields

		private ColumnInfo[] GetHeaderAndFields(ReportType type) {
			DataTable dt = DB.ExecuteReader("RptScheduler_GetHeaderAndFields", new QueryParameter("@Type", type.ToString()));

			string sHeader = null;
			string sFields = null;

			foreach (DataRow row in dt.Rows) {
				sHeader = row[0].ToString();
				sFields = row[1].ToString();
			} // for each

			return Report.ParseHeaderAndFields(sHeader, sFields);
		} // GetHeadersAndFields

		#endregion method GetHeaderAndFields
		
		#region report generators

		#region Earned Interest

		#region class EarnedInterestRow

		private class EarnedInterestRow {
			#region public

			#region fields

			public DateTime? IssueDate;
			public int ClientID;
			public int LoanID;
			public string ClientName;
			public string ClientEmail;
			public decimal EarnedInterest;
			public decimal LoanAmount;
			public decimal TotalRepaid;
			public decimal PrincipalRepaid;

			#endregion fields

			#region method Compare

			public static int Compare(EarnedInterestRow a, EarnedInterestRow b) {
				int c = DateTime.Compare((DateTime)a.IssueDate, (DateTime)b.IssueDate);

				if (c != 0)
					return c;

				return string.CompareOrdinal(a.ClientName, b.ClientName);
			} // Compare

			#endregion method Compare

			#region constructor

			public EarnedInterestRow(bool bIsTotal) {
				IsTotal = bIsTotal;
				IssueDate = null;
				ClientID = 0;
				LoanID = 0;
				ClientName = null;
				ClientEmail = null;
				EarnedInterest = 0;
				LoanAmount = 0;
				TotalRepaid = 0;
				PrincipalRepaid = 0;

				ClientCount = new SortedDictionary<int, int>();
				LoanCount = new SortedDictionary<int, int>();
			} // constructor

			#endregion constructor

			#region method Update

			public void Update(EarnedInterestRow v) {
				if (ClientCount.ContainsKey(v.ClientID))
					ClientCount[v.ClientID]++;
				else
					ClientCount[v.ClientID] = 1;

				if (LoanCount.ContainsKey(v.LoanID))
					LoanCount[v.LoanID]++;
				else
					LoanCount[v.LoanID] = 1;

				EarnedInterest  += v.EarnedInterest;
				LoanAmount      += v.LoanAmount;
				TotalRepaid     += v.TotalRepaid;
				PrincipalRepaid += v.PrincipalRepaid;
			} // Update

			#endregion method Update

			#region method ToRow

			public void ToRow(DataTable tbl) {
				if (IsTotal) {
					ClientID = ClientCount.Count;
					LoanID = LoanCount.Count;
				} // if

				tbl.Rows.Add(
					IssueDate, ClientID, LoanID, ClientName, ClientEmail,
					EarnedInterest, LoanAmount, TotalRepaid, PrincipalRepaid,
					IsTotal ? "total" : ""
				);
			} // ToRow

			#endregion method ToRow

			#region method ToTable

			public DataTable ToTable() {
				var oOutput = new DataTable();

				oOutput.Columns.Add("IssueDate", typeof(DateTime));
				oOutput.Columns.Add("ClientID", typeof(int));
				oOutput.Columns.Add("LoanID", typeof(int));
				oOutput.Columns.Add("ClientName", typeof(string));
				oOutput.Columns.Add("ClientEmail", typeof(string));
				oOutput.Columns.Add("EarnedInterest", typeof(double));
				oOutput.Columns.Add("LoanAmount", typeof(double));
				oOutput.Columns.Add("TotalRepaid", typeof(double));
				oOutput.Columns.Add("PrincipalRepaid", typeof(double));
				oOutput.Columns.Add("RowLevel", typeof(string));

				ToRow(oOutput);

				return oOutput;
			} // ToTable

			#endregion method ToTable

			#endregion public

			#region private

			private SortedDictionary<int, int> ClientCount;
			private SortedDictionary<int, int> LoanCount;
			private bool IsTotal;

			#endregion private
		} // class EarnedInterestRow

		#endregion class EarnedInterestRow

		private class LoanIntegrityRow
		{
			public int LoanId { get; set; }
			public decimal Diff { get; set; }

			public static int Compare(LoanIntegrityRow a, LoanIntegrityRow b)
			{
				if (a.Diff == b.Diff)
					return 0;
				if (a.Diff > b.Diff)
					return 1;
				return -1;
			}

			public void ToRow(DataTable tbl)
			{
				DataRow row = tbl.NewRow();
				row["LoanId"] = LoanId;
				row["Diff"] = Diff;
				tbl.Rows.Add(row);
			}
		}

		#region method CreateEarnedInterestReport

		private KeyValuePair<ReportQuery, DataTable> CreateEarnedInterestReport(Report report, DateTime today, DateTime tomorrow) {
			var ea = new EarnedInterest(DB, EarnedInterest.WorkingMode.ForPeriod, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			DataTable oData = rpt.Execute(DB);

			var oTotal = new EarnedInterestRow(true);

			var oRows = new List<EarnedInterestRow>();

			foreach (DataRow row in oData.Rows) {
				int nLoanID = Convert.ToInt32(row["LoanID"]);

				if (!earned.ContainsKey(nLoanID))
					continue;

				var oNewRow = new EarnedInterestRow(false) {
					IssueDate = Convert.ToDateTime(row["IssueDate"]),
					ClientID = Convert.ToInt32(row["ClientID"]),
					LoanID = nLoanID,
					ClientName = row["ClientName"].ToString(),
					ClientEmail = row["ClientEmail"].ToString(),
					EarnedInterest = earned[nLoanID],
					LoanAmount = f(row, "LoanAmount"),
					TotalRepaid = f(row, "TotalRepaid"),
					PrincipalRepaid = f(row, "PrincipalRepaid")
				};

				oTotal.Update(oNewRow);
				oRows.Add(oNewRow);
			} // for each earned interest

			oRows.Sort(EarnedInterestRow.Compare);

			DataTable oOutput = oTotal.ToTable();

			oRows.ForEach(r => r.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateEarnedInterestReport

		#endregion method CreateEarnedInterestReport

		private KeyValuePair<ReportQuery, DataTable> CreateLoanIntegrityReport(Report report)
		{
			var loanIntegrity = new LoanIntegrity(DB, this);
			SortedDictionary<int, decimal> diffs = loanIntegrity.Run();
			
			var oRows = new List<LoanIntegrityRow>();

			foreach (int loanId in diffs.Keys)
			{
				var oNewRow = new LoanIntegrityRow
				{
					LoanId = loanId,
					Diff = diffs[loanId]
				};
				oRows.Add(oNewRow);
			}

			oRows.Sort(LoanIntegrityRow.Compare);

			var oOutput = new DataTable();

			oOutput.Columns.Add("LoanID", typeof(int));
			oOutput.Columns.Add("Diff", typeof(decimal));

			oRows.ForEach(r => r.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(new ReportQuery(report), oOutput); // qqq - the new repoerQuery here is wrong
		}
		#endregion Earned Interest

		#region Loans Issued

		#region class LoansIssuedRow

		private class LoansIssuedRow {
			#region public

			#region constructor

			public LoansIssuedRow(DataRow row) {
				m_bIsTotal = row == null;

				m_oData = new SortedDictionary<string, dynamic>();

				if (m_bIsTotal) {
					m_oClients = new SortedDictionary<int, int>();

					foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames)
						m_oData[pair.Key] = pair.Value;

					m_oData[FldClientName] = "Total";
					m_oData[FldRowLevel] = "total";
				}
				else {
					foreach (KeyValuePair<string, dynamic> pair in ms_oFieldNames)
						m_oData[pair.Key] = row[pair.Key];
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

				foreach (string sIdx in new string[] { FldAccruedInterest, FldExpectedInterest, FldTotalInterest}) {
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

			private SortedDictionary<int, int> m_oClients;

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
				ms_oFieldNames[FldRowLevel] = "";

				ms_oTotalIgnored = new SortedDictionary<string, int>();
				ms_oTotalIgnored[FldLoanID] = 0;
				ms_oTotalIgnored[FldDate] = 0;
				ms_oTotalIgnored[FldClientID] = 0;
				ms_oTotalIgnored[FldClientEmail] = 0;
				ms_oTotalIgnored[FldClientName] = 0;
				ms_oTotalIgnored[FldLoanTypeName] = 0;
				ms_oTotalIgnored[FldDiscountPlan] = 0;
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
						oFldType = typeof (int);
						break;

					case FldDate:
						oFldType = typeof (DateTime);
						break;

					default:
						oFldType = typeof (string);
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

			DataTable tbl = rpt.Execute(DB);

			var ea = new EarnedInterest(DB, EarnedInterest.WorkingMode.ByIssuedLoans, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			var oRows = new List<LoansIssuedRow>();

			var oTotal = new LoansIssuedRow(null);

			foreach (DataRow row in tbl.Rows) {
				var lir = new LoansIssuedRow(row);
				oRows.Add(lir);

				lir.SetInterests(earned);

				oTotal.AddClient(lir);
				oTotal.AccumulateTotals(lir);
			} // for each row

			oTotal.SetLoanCount(tbl.Rows.Count);

			DataTable oOutput = oTotal.ToTable();

			oRows.ForEach(lir => lir.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateLoansIssuedReport

		#endregion method CreateLoansIssuedReport

		#endregion Loans Issued

		#region method CustomerReport

		private ATag CustomerReport(DateTime today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptCustomerReport", new QueryParameter("@DateStart", DB.DateToString(today)));

				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Email")))
					.Append(new Th().Append(new Text("Status")))
					.Append(new Th().Append(new Text("Wizard Finished")))
					.Append(new Th().Append(new Text("Account #")))
					.Append(new Th().Append(new Text("Credit Offer")))
					.Append(new Th().Append(new Text("Source Ad")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<Class>("Report").Append( new Thead().Append(oTr) );

				Tbody tbody = new Tbody();
				tbl.Append(tbody);

				foreach (DataRow row in dt.Rows) {
					oTr = new Tr()
						.Append(new Td().Add<Class>("L").Append(new Text(row["Name"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["Status"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["IsSuccessfullyRegistered"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["AccountNumber"].ToString())))
						.Append(new Td().Add<Class>("R").Append(new Text(row["CreditSum"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["ReferenceSource"].ToString())));

					tbody.Append(oTr);
				} // for each data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // CustomerReport

		#endregion method CustomerReport

		#region method AdsReport

		private ATag AdsReport(DateTime today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptAdsReport", new QueryParameter("@time", DB.DateToString(today)));

				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Ad Name")))
					.Append(new Th().Append(new Text("#")))
					.Append(new Th().Append(new Text("Total Credit Approved")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<Class>("Report").Append( new Thead().Append(oTr) );

				Tbody tbody = new Tbody();
				tbl.Append(tbody);

				foreach (DataRow row in dt.Rows) {
					oTr = new Tr()
						.Append(new Td().Add<Class>("L").Append(new Text(row["ReferenceSource"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["TotalUsers"].ToString())))
						.Append(new Td().Add<Class>("R").Append(new Text(row["TotalCredit"].ToString())));

					tbody.Append(oTr);
				} // foreach data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // AdsReport

		#endregion method AdsReport

		#region method PaymentReport

		private ATag PaymentReport(DateTime today) {
			Table tbl = new Table();

			try {
				DataTable dt = DB.ExecuteReader("RptPaymentReport",
					new QueryParameter("@DateStart", DB.DateToString(today)),
					new QueryParameter("@DateEnd", DB.DateToString(DateTime.Today.AddDays(3)))
				);

				ATag oTr = new Tr().Add<Class>("HR")
					.Append(new Th().Append(new Text("Id")))
					.Append(new Th().Append(new Text("Name")))
					.Append(new Th().Append(new Text("Email")))
					.Append(new Th().Append(new Text("Date")))
					.Append(new Th().Append(new Text("Amount")));

				oTr.ApplyToChildren<Class>("H");

				tbl.Add<ID>("tableReportData").Add<Class>("Report").Append( new Thead().Append(oTr) );

				var tbody = new Tbody();
				tbl.Append(tbody);

				foreach (DataRow row in dt.Rows) {
					oTr = new Tr()
						.Append(new Td().Add<Class>("L").Append(new Text(row["Id"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["Firstname"] + " " + row["SurName"])))
						.Append(new Td().Add<Class>("L").Append(new Text(row["Name"].ToString())))
						.Append(new Td().Add<Class>("L").Append(new Text(row["DATE"].ToString())))
						.Append(new Td().Add<Class>("R").Append(new Text(row["AmountDue"].ToString())));

					tbody.Append(oTr);
				} // foreach data row
			}
			catch (Exception e) {
				Error(e.ToString());
			}

			return tbl;
		} // PaymentReport

		#endregion method PaymentReport

		#endregion report generators

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
			if (oNumber is sbyte  ) return ((sbyte  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is byte   ) return ((byte   )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is short  ) return ((short  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is ushort ) return ((ushort )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is int    ) return ((int    )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is uint   ) return ((uint   )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is long   ) return ((long   )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is ulong  ) return ((ulong  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is float  ) return ((float  )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is double ) return ((double )oNumber).ToString(sFormat, FormatInfo);
			if (oNumber is decimal) return ((decimal)oNumber).ToString(sFormat, FormatInfo);

			throw new Exception(string.Format("Unsupported type: {0}", oNumber.GetType()));
		} // NumStr

		#endregion method NumStr

		#region method AddSheetToExcel

		private static Workbook AddSheetToExcel(DataTable dt, String title, String sheetName = "", String someText = "", Workbook wb = null) {
			InitAspose();

			const int fc = 1; // first column
			const int fr = 4; // first row
			const int frn = 1; // first row for title

			if (wb == null) { // first initialization, if we will use it multimple times.
				wb = new Workbook();
				wb.Worksheets.Clear();
			} // if

			if (String.IsNullOrEmpty(sheetName))
				sheetName = "Report"; // default sheetName

			var sheet = wb.Worksheets.Add(sheetName); // add new specific sheet to document.

			sheet.Cells.Merge(frn, fc, 1, dt.Columns.Count);
			sheet.Cells[frn, fc].PutValue(title.Replace("<h1>", "").Replace("</h1>", ""));
			sheet.Cells.Merge(frn + 1, fc, 1, dt.Columns.Count);
			sheet.Cells[frn + 1, fc].PutValue(someText);
			sheet.Cells.ImportDataTable(dt, true, fr, fc);
			sheet.AutoFitColumns();

			sheet.Cells.SetColumnWidth(0, 1);

			var titleStyle = sheet.Cells[fc, fr].GetStyle();
			var headerStyle = sheet.Cells[fc, fr].GetStyle();
			var lightStyle = sheet.Cells[fc, fr].GetStyle();
			var darkStyle = sheet.Cells[fc, fr].GetStyle();
			var footerStyle = sheet.Cells[fc, fr].GetStyle();

			titleStyle.VerticalAlignment = TextAlignmentType.Center;
			titleStyle.HorizontalAlignment = TextAlignmentType.Center;
			titleStyle.Pattern = BackgroundType.Solid;
			titleStyle.Font.IsBold = true;
			titleStyle.ForegroundColor = ColorTranslator.FromHtml("#EEEEEE");
			titleStyle.BackgroundColor = ColorTranslator.FromHtml("#EEEEEE");
			titleStyle.Font.Color = ColorTranslator.FromHtml("#666666");
			sheet.Cells[frn, fc].SetStyle(titleStyle);

			headerStyle.VerticalAlignment = TextAlignmentType.Center;
			headerStyle.HorizontalAlignment = TextAlignmentType.Center;
			headerStyle.Borders[BorderType.BottomBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Borders[BorderType.RightBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Borders[BorderType.TopBorder].Color = Color.Black;
			headerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;
			headerStyle.Pattern = BackgroundType.Solid;
			headerStyle.Font.IsBold = true;
			headerStyle.ForegroundColor = ColorTranslator.FromHtml("#9AB1D1");
			headerStyle.BackgroundColor = ColorTranslator.FromHtml("#9AB1D1");
			headerStyle.Font.Color = ColorTranslator.FromHtml("#FFFFFF");

			lightStyle.VerticalAlignment = TextAlignmentType.Center;
			lightStyle.HorizontalAlignment = TextAlignmentType.Left;
			lightStyle.Pattern = BackgroundType.Solid;
			lightStyle.Font.IsBold = false;
			lightStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
			lightStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
			lightStyle.Borders[BorderType.RightBorder].Color = Color.Black;
			lightStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
			lightStyle.ForegroundColor = ColorTranslator.FromHtml("#FFFFFF");
			lightStyle.BackgroundColor = ColorTranslator.FromHtml("#FFFFFF");
			lightStyle.Font.Color = ColorTranslator.FromHtml("#000000");

			darkStyle.VerticalAlignment = TextAlignmentType.Center;
			darkStyle.HorizontalAlignment = TextAlignmentType.Left;
			darkStyle.Pattern = BackgroundType.Solid;
			darkStyle.Font.IsBold = false;
			darkStyle.Borders[BorderType.LeftBorder].Color = Color.Black;
			darkStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Medium;
			darkStyle.Borders[BorderType.RightBorder].Color = Color.Black;
			darkStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Medium;
			darkStyle.ForegroundColor = ColorTranslator.FromHtml("#F9F9F9");
			darkStyle.BackgroundColor = ColorTranslator.FromHtml("#F9F9F9");
			darkStyle.Font.Color = ColorTranslator.FromHtml("#000000");

			footerStyle.Borders[BorderType.TopBorder].Color = Color.Black;
			footerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Medium;

			for (var it = fc; it < dt.Columns.Count + fc; it++)
				sheet.Cells[fr, it].SetStyle(headerStyle);

			for (var row = fr + 1; row <= dt.Rows.Count + fr; row++) {
				for (var column = fc; column < dt.Columns.Count + fc; column++)
					sheet.Cells[row, column].SetStyle(lightStyle);

				if (++row > (dt.Rows.Count + fr))
					continue;

				for (var column = fc; column < dt.Columns.Count + fc; column++)
					sheet.Cells[row, column].SetStyle(darkStyle);
			} // for

			for (var it = fc; it < dt.Columns.Count + fc; it++)
				sheet.Cells[fr + dt.Rows.Count + 1, it].SetStyle(footerStyle);

			return wb;
		} // AddSheetToExcel

		#endregion method AddSheetToExcel

		#region method f - Extract decimal from data row

		private static decimal f(DataRow row, string sFieldName) {
			return Convert.ToDecimal(row[sFieldName]);
		} // f

		#endregion method f - Extract decimal from data row

		private static readonly CultureInfo FormatInfo = new CultureInfo("en-GB");

		#endregion private static

		#endregion private
	} // class BaseReportHandler
} // namespace Reports
