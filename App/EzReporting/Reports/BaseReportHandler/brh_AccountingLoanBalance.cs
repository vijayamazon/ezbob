namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Html;
	using Html.Attributes;
	using Html.Tags;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		#region report generators

		#region method BuildAccountingLoanBalanceReport

		public ATag BuildAccountingLoanBalanceReport(Report report, DateTime today, DateTime tomorrow, List<string> oColumnTypes = null) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateAccountingLoanBalanceReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildAccountingLoanBalanceReport

		#endregion method BuildAccountingLoanBalanceReport

		#region method BuildAccountingLoanBalanceXls

		public ExcelPackage BuildAccountingLoanBalanceXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateAccountingLoanBalanceReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptEarnedInterest");
		} // BuildAccountingLoanBalanceXls

		#endregion method BuildAccountingLoanBalanceXls

		#region method CreateAccountingLoanBalanceReport

		private KeyValuePair<ReportQuery, DataTable> CreateAccountingLoanBalanceReport(Report report, DateTime today, DateTime tomorrow) {
			Debug("Creating accounting loan balance report...");

			Debug("Creating accounting loan balance report: loading earned interest...");

			var ea = new EarnedInterest.EarnedInterest(DB, EarnedInterest.EarnedInterest.WorkingMode.AccountingLoanBalance, true, today, tomorrow, this);
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

		#endregion method CreateAccountingLoanBalanceReport

		#endregion report generators

		#region helper classes

		#region class AccountingLoanBalanceRawUpdate

		private class AccountingLoanBalanceRawUpdate {
			[UsedImplicitly]
			public string LoanTranMethod { get; set; }

			[UsedImplicitly]
			public decimal TotalRepaid { get; set; }

			[UsedImplicitly]
			public decimal RolloverRepaid { get; set; }

			[UsedImplicitly]
			public int TransactionID { get; set; }

			[UsedImplicitly]
			public DateTime? TransactionDate { get; set; }

			[UsedImplicitly]
			public DateTime? LoanChargeDate { get; set; }

			[UsedImplicitly]
			public int FeesEarnedID { get; set; }

			[UsedImplicitly]
			public decimal FeesEarned { get; set; }
		} // AccountingLoanBalanceRawUpdate

		#endregion class AccountingLoanBalanceRawUpdate

		#region class AccountingLoanBalanceRawData

		private class AccountingLoanBalanceRawData : AccountingLoanBalanceRawUpdate {
			[UsedImplicitly]
			public DateTime IssueDate { get; set; }

			[UsedImplicitly]
			public int ClientID { get; set; }

			[UsedImplicitly]
			public int LoanID { get; set; }

			[UsedImplicitly]
			public string ClientName { get; set; }

			[UsedImplicitly]
			public string ClientEmail { get; set; }

			[UsedImplicitly]
			public decimal IssuedAmount { get; set; }

			[UsedImplicitly]
			public decimal SetupFee { get; set; }

			[UsedImplicitly]
			public string LoanStatus { get; set; }

			[UsedImplicitly]
			public decimal FeesRepaid { get; set; }
		} // AccountingLoanBalanceRawData

		#endregion class AccountingLoanBalanceRawData

		#region class AccountingLoanBalanceRow

		private class AccountingLoanBalanceRow {
			#region public

			#region constructor

			public AccountingLoanBalanceRow() {
				EarnedFees = 0;
				CashPaid = 0;
				WriteOffEarnedFees = 0;
				WriteOffCashPaid = 0;
				ClientID = 0;
				LoanID = 0;
				IssuedAmount = 0;
				SetupFee = 0;
				EarnedInterest = 0;
				NonCashPaid = 0;

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

				EarnedFees = 0;
				CashPaid = 0;
				WriteOffEarnedFees = 0;
				WriteOffCashPaid = 0;

				NonCashPaid = 0;

				if (SetupFee > 0)
					IssuedAmount -= SetupFee;

				LastInPeriodCustomerStatus = oLastChange.NewStatus;
				LastStatusChangeDate = oLastChange.ChangeDate;

				WriteOffDate = oWriteOffDate;

				Update(raw);
			} // constructor

			#endregion constructor

			#region method UpdateTotal

			public void UpdateTotal(AccountingLoanBalanceRow row) {
				LoanID++;

				IssuedAmount += row.IssuedAmount;
				SetupFee += row.SetupFee;
				EarnedInterest += row.EarnedInterest;
				WriteOffEarnedFees += row.WriteOffEarnedFees;
				WriteOffCashPaid += row.WriteOffCashPaid;

				if (row.CurrentCustomerStatus == CustomerStatus.WriteOff) {
					WriteOffTotalBalance +=
						row.IssuedAmount +
						row.SetupFee +
						row.EarnedInterest +
						row.WriteOffEarnedFees -
						row.WriteOffCashPaid;
				} // if

				EarnedFees += row.EarnedFees;
				CashPaid += row.CashPaid;
				NonCashPaid += row.NonCashPaid;
			} // UpdateTotal

			#endregion method UpdateTotal

			#region method Update

			public void Update(SafeReader sr) {
				Update(sr.Fill<AccountingLoanBalanceRawUpdate>());
			} // Update

			private void Update(AccountingLoanBalanceRawUpdate upd) {
				if (upd.TransactionDate.HasValue && !Transactions.Contains(upd.TransactionID)) {
					Transactions.Add(upd.TransactionID);

					bool bNonCash = (upd.LoanTranMethod ?? string.Empty).ToLower().StartsWith("non-cash");

					if (bNonCash)
						NonCashPaid += upd.TotalRepaid;
					else
						CashPaid += upd.TotalRepaid;

					EarnedFees += upd.RolloverRepaid;

					if (upd.TransactionDate.Value < WriteOffDate) {
						if (!bNonCash)
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

			#endregion method Update

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
				oOutput.Columns.Add("NonCashPaid", typeof(decimal));
				oOutput.Columns.Add("WriteOffBalance", typeof(decimal));
				oOutput.Columns.Add("Balance", typeof(decimal));
				oOutput.Columns.Add("LastInPeriodCustomerStatus", typeof(string));
				oOutput.Columns.Add("LastStatusChangeDate", typeof(DateTime));
				oOutput.Columns.Add("CurrentCustomerStatus", typeof(string));
				oOutput.Columns.Add("Css", typeof(string));

				return oOutput;
			} // ToTable

			#endregion method ToTable

			#region method ToRow

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
						SetupFee,
						EarnedInterest,
						EarnedFees,
						CashPaid,
						NonCashPaid,
						WriteOffTotalBalance,
						Balance,
						DBNull.Value,
						DBNull.Value,
						DBNull.Value,
						"total"
					);
				}
				else {
					tbl.Rows.Add(
						IssueDate,
						ClientID,
						LoanID,
						ClientName,
						ClientEmail,
						LoanStatus,
						IssuedAmount,
						SetupFee,
						EarnedInterest,
						EarnedFees,
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

			#endregion method ToRow

			#endregion public

			#region private

			#region property WriteOffBalance

			private decimal WriteOffBalance {
				get { return IssuedAmount + SetupFee + EarnedInterest + WriteOffEarnedFees - WriteOffCashPaid; } // get
			} // WriteOffBalance

			#endregion property WriteOffBalance

			#region property WriteOffTotalBalance

			private decimal WriteOffTotalBalance { get; set; } // WriteOffBalance

			#endregion property WriteOffTotalBalance

			#region property Balance

			private decimal Balance {
				get { return LastInPeriodCustomerStatus == CustomerStatus.WriteOff ? 0 : IssuedAmount + SetupFee + EarnedInterest + EarnedFees - CashPaid; } // get
			} // Balance

			#endregion property WriteOffBalance

			private SortedSet<int> Transactions { get; set; }
			private SortedSet<int> LoanCharges { get; set; }
			private DateTime IssueDate { get; set; }
			private int ClientID { get; set; }
			private int LoanID { get; set; }
			private string ClientName { get; set; }
			private string ClientEmail { get; set; }
			private decimal IssuedAmount { get; set; }
			private decimal SetupFee { get; set; }
			private string LoanStatus { get; set; }
			private decimal EarnedInterest { get; set; }
			private decimal EarnedFees { get; set; }
			private decimal CashPaid { get; set; }
			private decimal WriteOffEarnedFees { get; set; }
			private decimal WriteOffCashPaid { get; set; }
			private decimal NonCashPaid { get; set; }
			private CustomerStatus CurrentCustomerStatus { get; set; }
			private CustomerStatus LastInPeriodCustomerStatus { get; set; }
			private DateTime LastStatusChangeDate { get; set; }

			private DateTime WriteOffDate { get; set; }

			private bool IsTotal { get; set; }

			#endregion private
		} // class AccountingLoanBalanceRow

		#endregion class AccountingLoanBalanceRow

		#endregion helper classes
	} // class BaseReportHandler
} // namespace
