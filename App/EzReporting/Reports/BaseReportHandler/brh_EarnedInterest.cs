namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
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

		#region method CreateEarnedInterestReport

		private KeyValuePair<ReportQuery, DataTable> CreateEarnedInterestReport(Report report, bool bAccountingMode, DateTime today, DateTime tomorrow) {
			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.ForPeriod, bAccountingMode, today, tomorrow, this);
			SortedDictionary<int, decimal> earned = ea.Run();

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var oTotal = new EarnedInterestRow(true, CustomerStatus.Enabled, CustomerStatus.Enabled);

			var oRows = new List<EarnedInterestRow>();

			rpt.Execute(DB, (sr, bRowsetStart) => {
				int nLoanID = sr["LoanID"];

				if (!earned.ContainsKey(nLoanID))
					return ActionResult.Continue;

				int nClientID = sr["ClientID"];

				var oNewRow = new EarnedInterestRow(false, ea.CustomerStatusHistory.Data.GetLast(nClientID).NewStatus, ea.CustomerStatusHistory.GetCurrent(nClientID).NewStatus) {
					IssueDate = sr["IssueDate"],
					ClientID = nClientID,
					LoanID = nLoanID,
					ClientName = sr["ClientName"],
					ClientEmail = sr["ClientEmail"],
					EarnedInterest = earned[nLoanID],
					LoanAmount = sr["LoanAmount"],
					TotalRepaid = sr["TotalRepaid"],
					PrincipalRepaid = sr["PrincipalRepaid"],
					SetupFees = sr["SetupFees"],
					OtherFees = sr["OtherFees"],
					Rollover = sr["Rollover"],
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
			public decimal SetupFees;
			public decimal OtherFees;
			public decimal Rollover;
			private readonly CustomerStatus LastInPeriodStatus;
			private readonly CustomerStatus CurrentStatus;

			#endregion fields

			#region method Compare

			public static int Compare(EarnedInterestRow a, EarnedInterestRow b) {
				int c = DateTime.Compare(a.IssueDate, b.IssueDate);

				return (c != 0) ? c : string.CompareOrdinal(a.ClientName, b.ClientName);
			} // Compare

			#endregion method Compare

			#region constructor

			public EarnedInterestRow(bool bBIsTotal, CustomerStatus nLastInPeriodStatus, CustomerStatus nCurrentStatus) {
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
				m_oClientCount = new SortedDictionary<int, int>();
				m_oLoanCount = new SortedDictionary<int, int>();
				LastInPeriodStatus = nLastInPeriodStatus;
				CurrentStatus = nCurrentStatus;
				SetupFees = 0;
				OtherFees = 0;
				Rollover = 0;
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
				SetupFees       += v.SetupFees;
				OtherFees       += v.OtherFees;
				Rollover        += v.Rollover;
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
					EarnedInterest, LoanAmount, TotalRepaid, PrincipalRepaid,
					(m_bIsTotal ? "total" : ""),
					(m_bIsTotal ? string.Empty : LastInPeriodStatus.ToString()),
					(m_bIsTotal ? string.Empty : CurrentStatus.ToString()),
					SetupFees, OtherFees, Rollover, SetupFees + OtherFees + Rollover
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
				oOutput.Columns.Add("EarnedInterest", typeof(decimal));
				oOutput.Columns.Add("LoanAmount", typeof(decimal));
				oOutput.Columns.Add("TotalRepaid", typeof(decimal));
				oOutput.Columns.Add("PrincipalRepaid", typeof(decimal));
				oOutput.Columns.Add("RowLevel", typeof(string));
				oOutput.Columns.Add("LastInPeriodStatus", typeof(string));
				oOutput.Columns.Add("CurrentStatus", typeof(string));
				oOutput.Columns.Add("SetupFees", typeof(decimal));
				oOutput.Columns.Add("OtherFees", typeof(decimal));
				oOutput.Columns.Add("Rollover", typeof(decimal));
				oOutput.Columns.Add("TotalFees", typeof(decimal));

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

		#endregion private
	} // class BaseReportHandler
} // namespace Reports
