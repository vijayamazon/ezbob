namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.Strategies.Extensions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using OfficeOpenXml;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		int,
		System.Collections.Generic.List<LoanMetaData>
	>;

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	public class Datum {
		public Datum(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			ManualItems = new List<ManualDatumItem>();

			CustomerID = sr.CustomerID;
			BrokerID = sr.BrokerID;
			IsDefault = sr.IsDefault;

			Add(sr);

			Auto = new AutoDatumItem();
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			var mdi = new ManualDatumItem(sr);
			mdi.Calculate();
			ManualItems.Add(mdi);
		} // Add

		public string Tag { get; set; }
		public int CustomerID { get; set; }
		public int? BrokerID { get; set; }
		public bool IsDefault { get; set; }

		public List<ManualDatumItem> ManualItems { get; private set; }

		public ManualDatumItem FirstManual { get { return ManualItems[0]; } }
		public ManualDatumItem LastManual  { get { return ManualItems[ManualItems.Count - 1]; } }

		public AutoDatumItem Auto { get; private set; }

		public static string CsvTitles(SortedSet<string> sources) {
			/*
			var os = new List<string>();

			foreach (var s in sources) {
				os.Add(string.Format(
					"{0} loan count;{0} worst loan status;{0} issued amount;{0} repaid amount;{0} max late days",
					s
				));
			} // for each
			*/

			return string.Join(";",
				"Customer ID",
				"Broker ID",
				"Customer is default now",
				// "Has default loan",
				// "Loan was default",
				ManualDatumItem.CsvTitles("First"),
				ManualDatumItem.CsvTitles("Last"),
				ADatumItem.CsvTitles("Auto")/*,
				"Automation decision",
				"Auto re-reject decision",
				"Auto reject decision",
				"Auto re-approve decision",
				"Auto approve decision",
				"Re-approved amount",
				AMedalAndPricing.CsvTitles("Auto min"),
				"The same max offer",
				AMedalAndPricing.CsvTitles("Auto max"),
				string.Join(";", os)
				*/
			);
		} // CsvTitles

		/*
		public int LoanCount { get; private set; }
		public decimal LoanAmount { get; private set; }

		public bool HasDefaultLoan { get { return DefaultLoanCount > 0; } }
		public int DefaultLoanCount { get; private set; }
		public decimal DefaultLoanAmount { get; private set; }

		public bool HasBadLoan { get { return BadLoanCount > 0; } }
		public int BadLoanCount { get; private set; }
		public decimal BadLoanAmount { get; private set; }
		*/

		public int ToXlsx(ExcelWorksheet sheet, int rowNum, TCrLoans crLoans, SortedSet<string> sources) {
			/*
			List<LoanMetaData> lst = crLoans.ContainsKey(CashRequestID)
				? crLoans[CashRequestID]
				: new List<LoanMetaData>();

			var bySource = new SortedDictionary<string, LoanSummaryData>();

			foreach (string s in sources)
				bySource[s] = new LoanSummaryData();

			LoanCount = 0;
			LoanAmount = 0;

			DefaultLoanCount = 0;
			DefaultLoanAmount = 0;

			BadLoanCount = 0;
			BadLoanAmount = 0;

			foreach (LoanMetaData lmd in lst) {
				bySource[lmd.LoanSourceName].Add(lmd);

				LoanCount++;
				LoanAmount += lmd.LoanAmount;

				if (lmd.LoanStatus == LoanStatus.Late) {
					DefaultLoanCount++;
					DefaultLoanAmount += lmd.LoanAmount;
				} // if

				if (lmd.MaxLateDays > 13) {
					BadLoanCount++;
					BadLoanAmount += lmd.LoanAmount;
				} // if
			} // for
			*/

			int curColumn = 1;

			curColumn = sheet.SetCellValue(rowNum, curColumn, CustomerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, BrokerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsDefault ? "Default" : "No");
			// curColumn = sheet.SetCellValue(rowNum, curColumn, HasDefaultLoan ? "Default" : "No");
			// curColumn = sheet.SetCellValue(rowNum, curColumn, HasBadLoan ? "Default" : "No");

			curColumn = FirstManual.ToXlsx(sheet, rowNum, curColumn);
			curColumn = LastManual.ToXlsx(sheet, rowNum, curColumn);

			curColumn = Auto.ToXlsx(sheet, rowNum, curColumn);

			/*
			curColumn = sheet.SetCellValue(rowNum, curColumn, AutomationDecision.ToString());

			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoReRejected ? "Reject" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoRejected ? "Reject" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoReApproved ? "Approve" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsAutoApproved ? "Approve" : "Manual");
			curColumn = sheet.SetCellValue(rowNum, curColumn, ReapprovedAmount);

			curColumn = AutoMin.ToXlsx(sheet, rowNum, curColumn);

			curColumn = sheet.SetCellValue(rowNum, curColumn, (AutoMax == null) ? "Same" : "No");

			curColumn = (AutoMax ?? AutoMin).ToXlsx(sheet, rowNum, curColumn);

			foreach (string s in sources) {
				LoanSummaryData loanStat = bySource[s];
				curColumn = loanStat.ToXlsx(sheet, rowNum, curColumn);
			} // for each
			*/

			return curColumn;
		} // ToXlsx
	} // class Datum
} // namespace
