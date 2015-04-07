namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		long,
		System.Collections.Generic.List<LoanMetaData>
	>;

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	public class Datum {
		public Datum(SpLoadCashRequestsForAutomationReport.ResultRow sr, string tag, ASafeLog log) {
			this.tag = tag;
			this.log = log.Safe();

			ManualItems = new List<ManualDatumItem>();
			AutoItems = new List<AutoDatumItem>();
			LoanCount = new LoanCount(this.log);

			CustomerID = sr.CustomerID;
			BrokerID = sr.BrokerID;
			IsDefault = sr.IsDefault;

			Add(sr);
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			if (ManualItems.Count > 0) {
				if ((FirstManual.CustomerID != sr.CustomerID) || (FirstManual.IsApproved != sr.IsApproved)) {
					this.log.Alert(
						"Inconsistent customer id or manual decision while adding an item to datum. " +
						"Existing customer '{0}', is approved: '{1}'. " +
						"Appending customer '{2}', is approved: '{3}'.",
						FirstManual.CustomerID, FirstManual.IsApproved,
						sr.CustomerID, sr.IsApproved
					);
					throw new Exception("Inconsistent manual decision.");
				} // if
			} // if

			var mdi = new ManualDatumItem(sr, this.tag, this.log);
			mdi.Calculate();
			ManualItems.Add(mdi);
		} // Add

		/// <summary>
		/// Gets this datum manual decision. FirstManaul always exists because it must be specified in constructor,
		/// all the items share the same decision, so checking the first is enough.
		/// </summary>
		public bool IsApproved {
			get { return FirstManual.IsApproved; }
		} // IsApproved

		public string Tag { get; set; }
		public int CustomerID { get; set; }
		public int? BrokerID { get; set; }
		public bool IsDefault { get; set; }

		public LoanCount LoanCount { get; private set; }

		public List<ManualDatumItem> ManualItems { get; private set; }
		public List<AutoDatumItem> AutoItems { get; private set; }

		public ManualDatumItem Manual(int itemIndex) {
			if (itemIndex < 0)
				return ManualItems[ManualItems.Count + itemIndex];

			return ManualItems[itemIndex];
		} // Auto

		public AutoDatumItem Auto(int itemIndex) {
			if (itemIndex < 0)
				return AutoItems[AutoItems.Count + itemIndex];

			return AutoItems[itemIndex];
		} // Auto

		public void RunAutomation(bool isHomeOwner, AConnection db) {
			AutoItems.Clear();

			foreach (ManualDatumItem mi in ManualItems) {
				var ai = new AutoDatumItem(mi, this.tag, this.log);
				AutoItems.Add(ai);
				ai.RunAutomation(isHomeOwner, db);
				ai.SetAdjustedLoanCount(mi.LoanCount);
			} // for each manual item
		} // RunAutomation

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
				"Has default loan",
				// ManualDatumItem.CsvTitles("First"),
				"Decision count",
				ManualDatumItem.CsvTitles("Last"),
				// AutoDatumItem.CsvTitles("First"),
				AutoDatumItem.CsvTitles("Last"),
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

			LoanCount.Clear();

			foreach (ManualDatumItem mi in ManualItems) {
				long cashRequestID = mi.CashRequestID;

				if (!crLoans.ContainsKey(cashRequestID))
					continue;

				foreach (LoanMetaData lmd in crLoans[cashRequestID]) {
					this.loansBySource[lmd.LoanSourceName].Add(lmd);
					LoanCount += lmd;
					mi.LoanCount += lmd;
				} // for
			} // for each item
		} // FindLoans

		public int ToXlsx(ExcelWorksheet sheet, int rowNum) {
			int curColumn = 1;

			curColumn = sheet.SetCellValue(rowNum, curColumn, CustomerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, BrokerID);
			curColumn = sheet.SetCellValue(rowNum, curColumn, IsDefault ? "Default" : "No");
			curColumn = sheet.SetCellValue(rowNum, curColumn, LoanCount.Default.Exist ? "Default" : "No");

			// curColumn = FirstManual.ToXlsx(sheet, rowNum, curColumn);
			curColumn = sheet.SetCellValue(rowNum, curColumn, ManualItems.Count);
			curColumn = LastManual.ToXlsx(sheet, rowNum, curColumn);

			// curColumn = FirstAuto.ToXlsx(sheet, rowNum, curColumn);
			curColumn = LastAuto.ToXlsx(sheet, rowNum, curColumn);

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

		private ManualDatumItem FirstManual { get { return ManualItems[0]; } }
		private ManualDatumItem LastManual  { get { return ManualItems[ManualItems.Count - 1]; } }

		private AutoDatumItem FirstAuto { get { return AutoItems[0]; } }
		private AutoDatumItem LastAuto  { get { return AutoItems[AutoItems.Count - 1]; } }

		private SortedDictionary<string, LoanSummaryData> loansBySource;
		private readonly string tag;
		private readonly ASafeLog log;
	} // class Datum
} // namespace
