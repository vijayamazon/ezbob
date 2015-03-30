namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.ExcelExt;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using OfficeOpenXml;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		long,
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

			Auto = new AutoDatumItem(sr.CustomerID, sr.DecisionTime, sr.PreviousLoanCount);
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

		public int LoanCount { get; private set; }
		public decimal LoanAmount { get; private set; }

		public bool HasDefaultLoan { get { return DefaultLoanCount > 0; } }
		public int DefaultLoanCount { get; private set; }
		public decimal DefaultLoanAmount { get; private set; }

		public bool HasBadLoan { get { return BadLoanCount > 0; } }
		public int BadLoanCount { get; private set; }
		public decimal BadLoanAmount { get; private set; }

		public List<ManualDatumItem> ManualItems { get; private set; }

		public ManualDatumItem FirstManual { get { return ManualItems[0]; } }
		public ManualDatumItem LastManual  { get { return ManualItems[ManualItems.Count - 1]; } }

		public AutoDatumItem Auto { get; private set; }

		public static string CsvTitles(SortedSet<string> allLoanSources) {
			var os = new List<string>();

			foreach (var s in allLoanSources) {
				os.Add(string.Format(
					"{0} loan count;{0} worst loan status;{0} issued amount;{0} repaid amount;{0} max late days",
					s
				));
			} // for each

			return string.Join(";",
				"Customer ID",
				"Broker ID",
				"Customer is default now",
				"Has not cured loan",
				"Has 14 days late loan",
				ManualDatumItem.CsvTitles("First"),
				"Decision count",
				ManualDatumItem.CsvTitles("Last"),
				AutoDatumItem.CsvTitles("Auto"),
				/*
				AMedalAndPricing.CsvTitles("Auto min"),
				"The same max offer",
				AMedalAndPricing.CsvTitles("Auto max"),
				*/
				string.Join(";", os)
			);
		} // CsvTitles

		public void FindLoans(TCrLoans crLoans, SortedSet<string> allLoanSources) {
			this.loansBySource = new SortedDictionary<string, LoanSummaryData>();

			foreach (string s in allLoanSources)
				this.loansBySource[s] = new LoanSummaryData();

			LoanCount = 0;
			LoanAmount = 0;

			DefaultLoanCount = 0;
			DefaultLoanAmount = 0;

			BadLoanCount = 0;
			BadLoanAmount = 0;

			foreach (ManualDatumItem mi in ManualItems) {
				long cashRequestID = mi.CashRequestID;

				if (!crLoans.ContainsKey(cashRequestID))
					continue;

				foreach (LoanMetaData lmd in crLoans[cashRequestID]) {
					this.loansBySource[lmd.LoanSourceName].Add(lmd);

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
			} // for each item
		} // FindLoans

		public int ToXlsx(ExcelWorksheet sheet, int rowNum) {
			int curColumn = 1;

			curColumn = sheet.SetCellValue(rowNum, curColumn, CustomerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, BrokerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsDefault ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, HasDefaultLoan ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, HasBadLoan ? "Default" : "No");

			curColumn = FirstManual.ToXlsx(sheet, rowNum, curColumn);
			curColumn = sheet.SetCellValue(rowNum, curColumn, ManualItems.Count);
			curColumn = LastManual.ToXlsx(sheet, rowNum, curColumn);

			curColumn = Auto.ToXlsx(sheet, rowNum, curColumn);

			/*
			curColumn = AutoMin.ToXlsx(sheet, rowNum, curColumn);

			curColumn = sheet.SetCellValue(rowNum, curColumn, (AutoMax == null) ? "Same" : "No");

			curColumn = (AutoMax ?? AutoMin).ToXlsx(sheet, rowNum, curColumn);
			*/

			foreach (var pair in this.loansBySource) {
				LoanSummaryData loanStat = pair.Value;
				curColumn = loanStat.ToXlsx(sheet, rowNum, curColumn);
			} // for each

			return curColumn;
		} // ToXlsx

		private SortedDictionary<string, LoanSummaryData> loansBySource;
	} // class Datum
} // namespace
