namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Html;
	using Html.Attributes;
	using Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		#region public

		#region method BuildEarnedInterestReport

		public ATag BuildEarnedInterestReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateEarnedInterestReport(report, false, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildEarnedInterestReport

		#endregion method BuildEarnedInterestReport

		#region method BuildEarnedInterestAllCustomersReport

		public ATag BuildEarnedInterestAllCustomersReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateEarnedInterestReport(report, true, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildEarnedInterestAllCustomersReport

		#endregion method BuildEarnedInterestAllCustomersReport

		#region method BuildEarnedInterestXls

		public ExcelPackage BuildEarnedInterestXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateEarnedInterestReport(report, false, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildEarnedInterestXls

		#endregion method BuildEarnedInterestXls

		#region method BuildEarnedInterestAllCustomersXls

		public ExcelPackage BuildEarnedInterestAllCustomersXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateEarnedInterestReport(report, true, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildEarnedInterestXls

		#endregion method BuildEarnedInterestAllCustomersXls

		#endregion public

		#region private

		#region class EarnedInterestRow

		private class EarnedInterestRow {
			#region public

			#region fields

			public DateTime IssueDate;
			public int ClientID;
			public int LoanID;
			public string ClientName;
			public string ClientEmail;
			public decimal EarnedInterest;
			public decimal LoanAmount;
			public decimal TotalRepaid;
			public decimal PrincipalRepaid;
			public string CustomerStatus;

			#endregion fields

			#region method Compare

			public static int Compare(EarnedInterestRow a, EarnedInterestRow b) {
				int c = DateTime.Compare(a.IssueDate, b.IssueDate);

				return (c != 0) ? c : string.CompareOrdinal(a.ClientName, b.ClientName);
			} // Compare

			#endregion method Compare

			#region constructor

			public EarnedInterestRow(bool bBIsTotal) {
				m_bIsTotal = bBIsTotal;
				IssueDate = ms_oLongAgo;
				ClientID = 0;
				LoanID = 0;
				ClientName = null;
				ClientEmail = null;
				EarnedInterest = 0;
				LoanAmount = 0;
				TotalRepaid = 0;
				PrincipalRepaid = 0;
				CustomerStatus = "";
				m_oClientCount = new SortedDictionary<int, int>();
				m_oLoanCount = new SortedDictionary<int, int>();
			} // constructor

			#endregion constructor

			#region method Update

			public void Update(EarnedInterestRow v) {
				if (m_oClientCount.ContainsKey(v.ClientID))
					m_oClientCount[v.ClientID]++;
				else
					m_oClientCount[v.ClientID] = 1;

				if (m_oLoanCount.ContainsKey(v.LoanID))
					m_oLoanCount[v.LoanID]++;
				else
					m_oLoanCount[v.LoanID] = 1;

				EarnedInterest  += v.EarnedInterest;
				LoanAmount      += v.LoanAmount;
				TotalRepaid     += v.TotalRepaid;
				PrincipalRepaid += v.PrincipalRepaid;
			} // Update

			#endregion method Update

			#region method ToRow

			public void ToRow(DataTable tbl) {
				if (m_bIsTotal) {
					ClientID = m_oClientCount.Count;
					LoanID = m_oLoanCount.Count;
				} // if

				tbl.Rows.Add(
					(IssueDate == ms_oLongAgo ? (DateTime?)null : IssueDate), ClientID, LoanID, ClientName, ClientEmail,
					EarnedInterest, LoanAmount, TotalRepaid, PrincipalRepaid,CustomerStatus,
					(m_bIsTotal ? "total" : "")
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
				oOutput.Columns.Add("CustomerStatus", typeof(string));
				oOutput.Columns.Add("RowLevel", typeof(string));

				ToRow(oOutput);

				return oOutput;
			} // ToTable

			#endregion method ToTable

			#endregion public

			#region private

			private readonly SortedDictionary<int, int> m_oClientCount;
			private readonly SortedDictionary<int, int> m_oLoanCount;
			private readonly bool m_bIsTotal;

			private static readonly DateTime ms_oLongAgo = new DateTime(1991, 8, 24, 0, 0, 0, DateTimeKind.Utc);

			#endregion private
		} // class EarnedInterestRow

		#endregion class EarnedInterestRow

		#region method CreateEarnedInterestReport

		private KeyValuePair<ReportQuery, DataTable> CreateEarnedInterestReport(Report report, bool bIgnoreCustomerStatus, DateTime today, DateTime tomorrow) {
			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.ForPeriod, bIgnoreCustomerStatus, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var oTotal = new EarnedInterestRow(true);

			var oRows = new List<EarnedInterestRow>();

			rpt.Execute(DB, (sr, bRowsetStart) => {
				int nLoanID = sr["LoanID"];

				if (!earned.ContainsKey(nLoanID))
					return ActionResult.Continue;

				var oNewRow = new EarnedInterestRow(false) {
					IssueDate = sr["IssueDate"],
					ClientID = sr["ClientID"],
					LoanID = nLoanID,
					ClientName = sr["ClientName"],
					ClientEmail = sr["ClientEmail"],
					EarnedInterest = earned[nLoanID],
					LoanAmount = sr["LoanAmount"],
					TotalRepaid = sr["TotalRepaid"],
					PrincipalRepaid = sr["PrincipalRepaid"],
					CustomerStatus = sr["CustomerStatus"],
				};

				oTotal.Update(oNewRow);
				oRows.Add(oNewRow);

				return ActionResult.Continue;
			}); // for each earned interest

			oRows.Sort(EarnedInterestRow.Compare);

			DataTable oOutput = oTotal.ToTable();

			oRows.ForEach(r => r.ToRow(oOutput));

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateEarnedInterestReport

		#endregion method CreateEarnedInterestReport

		#endregion private
	} // class BaseReportHandler
} // namespace Reports
