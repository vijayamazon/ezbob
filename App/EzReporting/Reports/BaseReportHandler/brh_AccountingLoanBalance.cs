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
	} // class BaseReportHandler
} // namespace
