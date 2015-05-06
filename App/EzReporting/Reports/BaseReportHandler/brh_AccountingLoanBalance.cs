namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		public ATag BuildAccountingLoanBalanceReport(
			Report report,
			DateTime today,
			DateTime tomorrow,
			List<string> oColumnTypes = null
		) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateAccountingLoanBalanceReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildAccountingLoanBalanceReport

		public ExcelPackage BuildAccountingLoanBalanceXls(
			Report report,
			DateTime today,
			DateTime tomorrow
		) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateAccountingLoanBalanceReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildAccountingLoanBalanceXls

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class AccountingLoanBalanceRawUpdate {
			public string LoanTranMethod { get; set; }
			public decimal TotalRepaid { get; set; }
			public decimal RepaidPrincipal { get; set; }
			public decimal RepaidInterest { get; set; }
			public decimal RolloverRepaid { get; set; }
			public int TransactionID { get; set; }
			public DateTime? TransactionDate { get; set; }
			public DateTime? LoanChargeDate { get; set; }
			public int FeesEarnedID { get; set; }
			public decimal FeesEarned { get; set; }
			public decimal FeesRepaid { get; set; }
		} // class AccountingLoanBalanceRawUpdate

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class AccountingLoanBalanceRawData : AccountingLoanBalanceRawUpdate {
			public DateTime IssueDate { get; set; }
			public int ClientID { get; set; }
			public int LoanID { get; set; }
			public string ClientName { get; set; }
			public string ClientEmail { get; set; }
			public decimal IssuedAmount { get; set; }
			public decimal SetupFee { get; set; }
			public string LoanStatus { get; set; }
		} // class AccountingLoanBalanceRawData

		private class AccountingLoanBalanceRow {
			public AccountingLoanBalanceRow() {
				EarnedFees = 0;
				CashPaid = 0;
				WriteOffEarnedFees = 0;
				WriteOffCashPaid = 0;
				WriteOffNonCashPaid = 0;
				ClientID = 0;
				LoanID = 0;
				IssuedAmount = 0;
				RepaidPrincipal = 0;
				RepaidInterest = 0;
				FeesRepaid = 0;
				SetupFee = 0;
				EarnedInterest = 0;
				NonCashPaid = 0;

				WriteOffTotalBalance = 0;
				TotalBalance = 0;

				IsTotal = true;
			} // constructor

			public AccountingLoanBalanceRow(
				SafeReader sr,
				decimal nEarnedInterest,
				CustomerStatusChange oLastChange,
				CustomerStatusChange oCurrent,
				DateTime oWriteOffDate
			) {
				IsTotal = false;

				WriteOffTotalBalance = 0;
				TotalBalance = 0;

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
				LoanStatus = raw.LoanStatus;
				CurrentCustomerStatus = oCurrent.NewStatus;
				EarnedInterest = nEarnedInterest;

				RepaidPrincipal = 0;
				RepaidInterest = 0;
				FeesRepaid = 0;
				EarnedFees = 0;
				CashPaid = 0;
				WriteOffEarnedFees = 0;
				WriteOffCashPaid = 0;
				WriteOffNonCashPaid = 0;

				NonCashPaid = 0;

				if (SetupFee > 0)
					IssuedAmount -= SetupFee;

				LastInPeriodCustomerStatus = oLastChange.NewStatus;
				LastStatusChangeDate = oLastChange.ChangeDate;

				WriteOffDate = oWriteOffDate;

				Update(null, raw);
			} // constructor

			public static DataTable ToTable() {
				var oOutput = new DataTable();

				oOutput.Columns.Add("IssueDate", typeof(DateTime));
				oOutput.Columns.Add("ClientID", typeof(int));
				oOutput.Columns.Add("LoanID", typeof(int));
				oOutput.Columns.Add("ClientName", typeof(string));
				oOutput.Columns.Add("ClientEmail", typeof(string));
				oOutput.Columns.Add("LoanStatus", typeof(string));
				oOutput.Columns.Add("IssuedAmount", typeof(decimal));
				oOutput.Columns.Add("OutstandingPrincipal", typeof(decimal));
				oOutput.Columns.Add("SetupFee", typeof(decimal));
				oOutput.Columns.Add("EarnedInterest", typeof(decimal));
				oOutput.Columns.Add("OutstandingInterest", typeof(decimal));
				oOutput.Columns.Add("EarnedFees", typeof(decimal));
				oOutput.Columns.Add("OutstandingFees", typeof(decimal));
				oOutput.Columns.Add("CashPaid", typeof(decimal));
				oOutput.Columns.Add("NonCashPaid", typeof(decimal));
				oOutput.Columns.Add("WriteOffBalance", typeof(decimal));
				oOutput.Columns.Add("Balance", typeof(decimal));
				oOutput.Columns.Add("LastInPeriodCustomerStatus", typeof(string));
				oOutput.Columns.Add("LastStatusChangeDate", typeof(DateTime));
				oOutput.Columns.Add("CurrentCustomerStatus", typeof(string));
				oOutput.Columns.Add("Css", typeof(string));

				return oOutput;
			} // ToTable

			public void UpdateTotal(AccountingLoanBalanceRow row) {
				LoanID++;

				IssuedAmount += row.IssuedAmount;
				RepaidPrincipal += row.RepaidPrincipal;
				RepaidInterest += row.RepaidInterest;
				FeesRepaid += row.FeesRepaid;
				SetupFee += row.SetupFee;
				EarnedInterest += row.EarnedInterest;

				EarnedFees += row.EarnedFees;
				CashPaid += row.CashPaid;
				NonCashPaid += row.NonCashPaid;

				WriteOffEarnedFees += row.WriteOffEarnedFees;
				WriteOffCashPaid += row.WriteOffCashPaid;
				WriteOffNonCashPaid += row.WriteOffNonCashPaid;

				TotalBalance += row.Balance;

				if (row.CurrentCustomerStatus == CustomerStatus.WriteOff) {
					WriteOffTotalBalance +=
						row.IssuedAmount +
							row.SetupFee +
							row.EarnedInterest +
							row.WriteOffEarnedFees -
							row.WriteOffCashPaid -
							row.WriteOffNonCashPaid;
				} // if
			} // UpdateTotal

			public void Update(SafeReader sr, AccountingLoanBalanceRawUpdate upd = null) {
				upd = upd ?? sr.Fill<AccountingLoanBalanceRawUpdate>();

				if (upd.TransactionDate.HasValue && !Transactions.Contains(upd.TransactionID)) {
					Transactions.Add(upd.TransactionID);

					bool bNonCash = (upd.LoanTranMethod ?? string.Empty).ToLower().StartsWith("non-cash");

					if (bNonCash)
						NonCashPaid += upd.TotalRepaid;
					else
						CashPaid += upd.TotalRepaid;

					EarnedFees += upd.RolloverRepaid;
					RepaidPrincipal += upd.RepaidPrincipal;
					RepaidInterest += upd.RepaidInterest;
					FeesRepaid += upd.FeesRepaid;

					if (upd.TransactionDate.Value < WriteOffDate) {
						if (bNonCash)
							WriteOffNonCashPaid += upd.TotalRepaid;
						else
							WriteOffCashPaid += upd.TotalRepaid;

						WriteOffEarnedFees += upd.RolloverRepaid;
					} // if
				} // if

				if (upd.LoanChargeDate.HasValue && !LoanCharges.Contains(upd.FeesEarnedID)) {
					LoanCharges.Add(upd.FeesEarnedID);
					EarnedFees += upd.FeesEarned;

					if (upd.LoanChargeDate < WriteOffDate)
						WriteOffEarnedFees += upd.FeesEarned;
				} // if
			} // Update

			public void ToRow(DataTable tbl) {
				if (IsTotal) {
					tbl.Rows.Add(
						DBNull.Value,
						DBNull.Value,
						LoanID,
						"Total",
						DBNull.Value,
						DBNull.Value,
						IssuedAmount,
						IssuedAmount - RepaidPrincipal,
						SetupFee,
						EarnedInterest,
						EarnedInterest - RepaidInterest,
						EarnedFees,
						EarnedFees - FeesRepaid,
						CashPaid,
						NonCashPaid,
						WriteOffTotalBalance,
						TotalBalance,
						DBNull.Value,
						DBNull.Value,
						DBNull.Value,
						"total"
						);
				} else {
					tbl.Rows.Add(
						IssueDate,
						ClientID,
						LoanID,
						ClientName,
						ClientEmail,
						LoanStatus,
						IssuedAmount,
						IssuedAmount - RepaidPrincipal,
						SetupFee,
						EarnedInterest,
						EarnedInterest - RepaidInterest,
						EarnedFees,
						EarnedFees - FeesRepaid,
						CashPaid,
						NonCashPaid,
						CurrentCustomerStatus == CustomerStatus.WriteOff ? (object)WriteOffBalance : (object)DBNull.Value,
						Balance,
						LastInPeriodCustomerStatus.ToString(),
						LastStatusChangeDate,
						CurrentCustomerStatus.ToString(),
						string.Empty
					);
				} // if
			} // ToRow

			private decimal WriteOffBalance {
				get {
					return IssuedAmount +
						SetupFee +
						EarnedInterest +
						WriteOffEarnedFees -
						WriteOffCashPaid -
						WriteOffNonCashPaid;
				} // get
			} // WriteOffBalance

			private decimal WriteOffTotalBalance { get; set; }

			private decimal TotalBalance { get; set; }

			private decimal Balance {
				get {
					return LastInPeriodCustomerStatus == CustomerStatus.WriteOff
						? 0
						: IssuedAmount + SetupFee + EarnedInterest + EarnedFees - CashPaid - NonCashPaid;
				} // get
			} // Balance

			private SortedSet<int> Transactions { get; set; }
			private SortedSet<int> LoanCharges { get; set; }
			private DateTime IssueDate { get; set; }
			private int ClientID { get; set; }
			private int LoanID { get; set; }
			private string ClientName { get; set; }
			private string ClientEmail { get; set; }
			private decimal IssuedAmount { get; set; }
			private decimal RepaidPrincipal { get; set; }
			private decimal SetupFee { get; set; }
			private string LoanStatus { get; set; }
			private decimal EarnedInterest { get; set; }
			private decimal RepaidInterest { get; set; }
			private decimal FeesRepaid { get; set; }
			private decimal EarnedFees { get; set; }
			private decimal CashPaid { get; set; }
			private decimal WriteOffEarnedFees { get; set; }
			private decimal WriteOffCashPaid { get; set; }
			private decimal WriteOffNonCashPaid { get; set; }
			private decimal NonCashPaid { get; set; }
			private CustomerStatus CurrentCustomerStatus { get; set; }
			private CustomerStatus LastInPeriodCustomerStatus { get; set; }
			private DateTime LastStatusChangeDate { get; set; }
			private DateTime WriteOffDate { get; set; }
			private bool IsTotal { get; set; }
		} // class AccountingLoanBalanceRow

		private KeyValuePair<ReportQuery, DataTable> CreateAccountingLoanBalanceReport(
			Report report,
			DateTime today,
			DateTime tomorrow
		) {
			Debug("Creating accounting loan balance report...");

			Debug("Creating accounting loan balance report: loading earned interest...");

			var ea = new EarnedInterest.EarnedInterest(
				DB,
				EarnedInterest.EarnedInterest.WorkingMode.ForPeriod,
				true,
				today,
				tomorrow,
				this
			);
			SortedDictionary<int, decimal> earned = ea.Run();

			Debug("Creating accounting loan balance report: loading earned interest complete.");

			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var oRows = new SortedDictionary<int, AccountingLoanBalanceRow>();

			Debug("Creating accounting loan balance report: loading report data...");

			rpt.Execute(DB, (sr, bRowsetStart) => {
				int nLoanID = sr["LoanID"];

				decimal nEarnedInterest = earned.ContainsKey(nLoanID) ? earned[nLoanID] : 0;

				if (oRows.ContainsKey(nLoanID))
					oRows[nLoanID].Update(sr);
				else {
					int nClientID = sr["ClientID"];
					oRows[nLoanID] = new AccountingLoanBalanceRow(
						sr,
						nEarnedInterest,
						ea.CustomerStatusHistory.Data.GetLast(nClientID),
						ea.CustomerStatusHistory.GetCurrent(nClientID),
						ea.CustomerStatusHistory.Data.GetWriteOffDate(nClientID) ?? tomorrow
					);
				} // if

				return ActionResult.Continue;
			});

			Debug("Creating accounting loan balance report: loading report data complete.");

			Debug("Creating accounting loan balance report: creating an output...");

			DataTable oOutput = AccountingLoanBalanceRow.ToTable();

			Debug("Creating accounting loan balance report: table is ready, filling it...");

			var oTotal = new AccountingLoanBalanceRow();

			foreach (KeyValuePair<int, AccountingLoanBalanceRow> pair in oRows)
				oTotal.UpdateTotal(pair.Value);

			oTotal.ToRow(oOutput);

			foreach (KeyValuePair<int, AccountingLoanBalanceRow> pair in oRows)
				pair.Value.ToRow(oOutput);

			Debug("Creating accounting loan balance report complete.");

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateAccountingLoanBalanceReport
	} // class BaseReportHandler
} // namespace
