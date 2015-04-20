namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;
	using PaymentServices.Calculators;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		long,
		System.Collections.Generic.List<LoanMetaData>
	>;

	public class MaamMedalAndPricing : AStrategy {
		public MaamMedalAndPricing() {
			this.spLoad = new SpLoadCashRequestsForAutomationReport(DB, Log);

			Data = new List<Datum>();

			this.homeOwners = new SortedDictionary<int, bool>();
			this.crLoans = new TCrLoans();
			this.loanSources = new SortedSet<string>();

			this.tag = string.Format(
				"#MaamMedalAndPricing_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid()
					.ToString("N")
				);

			Log.Debug("The tag is '{0}'.", this.tag);
		} // constructor

		public override string Name {
			get { return "MaamMedalAndPricing"; }
		} // Name

		public List<Datum> Data { get; private set; }

		public override void Execute() {
			this.loanSources.Clear();
			this.crLoans.Clear();

			DB.ForEachResult<LoanMetaData>(
				lmd => {
					if (this.crLoans.ContainsKey(lmd.CashRequestID))
						this.crLoans[lmd.CashRequestID].Add(lmd);
					else
						this.crLoans[lmd.CashRequestID] = new List<LoanMetaData> {
							lmd
						};

					this.loanSources.Add(lmd.LoanSourceName);
				},
				"LoadAllLoansMetaData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Today", spLoad.DateTo ?? new DateTime(2015, 4, 1, 0, 0, 0, DateTimeKind.Utc))
				);

			LoadCashRequests();

			var pc = new ProgressCounter("{0} cash requests processed.", Log, 50);

			foreach (Datum d in Data) {
				bool isHomeOwner = IsHomeOwner(d.CustomerID);

				try {
					d.RunAutomation(isHomeOwner, DB);
				} catch (Exception e) {
					Log.Alert(e, "Automation failed for customer {0}.", d.CustomerID);
				} // try

				pc++;
			} // for

			pc.Log();

			CsvTitles = Datum.CsvTitles(this.loanSources).Split(';');
			CreateXlsx();
		} // Execute

		public virtual string[] CsvTitles { get; private set; }

		public virtual ExcelPackage Xlsx { get; private set; }

		private void CreateXlsx() {
			Xlsx = new ExcelPackage();

			ExcelWorksheet verificationSheet = Xlsx.CreateSheet("Verification", false);
			ExcelWorksheet minOfferStatSheet = Xlsx.CreateSheet("Min offer", false);
			ExcelWorksheet maxOfferStatSheet = Xlsx.CreateSheet("Max offer", false);
			ExcelWorksheet decisionSheet = Xlsx.CreateSheet(DecisionsSheetName, false);
			ExcelWorksheet loanIDSheet = Xlsx.CreateSheet("Loan IDs", false);

			AppendDecisionTitles(decisionSheet);

			var decisionStats = new Stats(Log, minOfferStatSheet, true, minOfferFormulaeSheet, Color.Yellow);

			var stats = new List<Tuple<Stats, int>> {
				new Tuple<Stats, int>(decisionStats, -1),
				new Tuple<Stats, int>(new Stats(Log, maxOfferStatSheet, false, maxOfferFormulaeSheet, Color.LawnGreen), -1),
			};

			var pc = new ProgressCounter("{0} items sent to .xlsx", Log, 50);

			int curRow = 2;

			foreach (Datum d in Data) {
				d.ToXlsx(decisionSheet, curRow);
				curRow++;

				foreach (Tuple<Stats, int> pair in stats)
					pair.Item1.Add(d, pair.Item2);

				pc++;
			} // for each

			pc.Log();

			int lastDecisionRow = curRow - 1;

			curRow = DrawVerificationData(verificationSheet, 1, lastDecisionRow);

			curRow = DrawConfiguration(verificationSheet, curRow + 1);

			DrawTotalAndReject(verificationSheet, curRow + 1, lastDecisionRow);

			int loanIDColumn = 1;

			foreach (Tuple<Stats, int> pair in stats) {
				pair.Item1.ToXlsx(1, lastDecisionRow);

				loanIDColumn = pair.Item1.FlushLoanIDs(loanIDSheet, loanIDColumn);
			} // for each

			// Currently (Apr 20 2015) the last column is CD in Decisions, so 90 should be good enough.
			Xlsx.AutoFitColumns(90);
		} // CreateXlsx

		protected virtual TCrLoans CashRequestLoans {
			get { return this.crLoans; }
		} // CashRequestLoans

		protected virtual SortedSet<string> LoanSources {
			get { return this.loanSources; }
		} // LoanSources

		protected virtual List<int> RequestedCustomers {
			get { return this.spLoad.RequestedCustomers; }
		} // CustomerID

		protected virtual DateTime? DateFrom {
			get { return this.spLoad.DateFrom; }
			set { this.spLoad.DateFrom = value; }
		} // DateFrom

		protected virtual DateTime? DateTo {
			get { return this.spLoad.DateTo; }
			set { this.spLoad.DateTo = value; }
		} // DateTo

		private static int DrawVerificationData(ExcelWorksheet statSheet, int row, int lastRawRow) {
			AStatItem.SetBorders(statSheet.Cells[row, 1, row, 3]).Merge = true;
			statSheet.SetCellValue(row, 1, "Verification data", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			statSheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			statSheet.SetCellValue(row, 2, "Reference", true);
			statSheet.SetCellValue(row, 3, "Actual", true);
			row++;

			row = DrawVerificationRow(
				statSheet,
				row,
				"Approve count",
				Reference.Approve.Count,
				string.Format("=SUMIF(Decisions!$D$2:$D${0}, \"Default\",Decisions!$BD$2:$BD${0})", lastRawRow),
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Approve amount",
				Reference.Approve.Amount,
				string.Format("=SUMIF(Decisions!$J$2:$J${0},\"Approved\",Decisions!$K$2:$K${0})", lastRawRow),
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Loan count",
				Reference.Loan.Count,
				string.Format("=SUM(Decisions!$BA$2:$BA${0})", lastRawRow),
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Loan amount",
				Reference.Loan.Amount,
				string.Format("=SUM(Decisions!$BB$2:$BB${0})", lastRawRow),
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Default count",
				Reference.Default.Count,
				string.Format("=COUNTIF(Decisions!$D$2:$D${0}, \"Default\")", lastRawRow),
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Default issued amount",
				Reference.Default.Issued.Amount,
				string.Format("=SUMIF(Decisions!$D$2:$D${0}, \"Default\",Decisions!$BB$2:$BB${0})", lastRawRow),
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Default outstanding amount",
				Reference.Default.Outstanding.Amount,
				string.Format("=SUMIF(Decisions!$D$2:$D${0}, \"Default\",Decisions!$BD$2:$BD${0})", lastRawRow),
				TitledValue.Format.Money
			);

			return row;
		} // DrawVerificationData

		private static int DrawConfiguration(ExcelWorksheet cfgSheet, int row) {
			AStatItem.SetBorders(cfgSheet.Cells[row, 1, row, 2]).Merge = true;
			cfgSheet.SetCellValue(row, 1, "Report configuration", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			cfgSheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			cfgSheet.SetCellValue(row, 1, "First auto approve top limitation");
			cfgSheet.SetCellValue(row, 2, 15000, sNumberFormat: TitledValue.Format.Money);
			row++;

			cfgSheet.SetCellValue(row, 1, "Second auto approve top limitation");
			cfgSheet.SetCellValue(row, 2, 20000, sNumberFormat: TitledValue.Format.Money);
			row++;

			cfgSheet.SetCellValue(row, 1, "Default issued rate (% of loans) - amount");
			cfgSheet.SetCellValue(row, 2, 0.2, sNumberFormat: TitledValue.Format.Percent);
			row++;

			cfgSheet.SetCellValue(row, 1, "Default issued rate (% of loans) - count");
			cfgSheet.SetCellValue(row, 2, 0.2, sNumberFormat: TitledValue.Format.Percent);
			row++;

			cfgSheet.SetCellValue(row, 1, "Home owner cap");
			cfgSheet.SetCellValue(row, 2, 120000, sNumberFormat: TitledValue.Format.Money);
			row++;

			cfgSheet.SetCellValue(row, 1, "Homeless cap");
			cfgSheet.SetCellValue(row, 2, 20000, sNumberFormat: TitledValue.Format.Money);
			row++;

			cfgSheet.SetCellValue(row, 1, "Round to");
			cfgSheet.SetCellValue(row, 2, 100, sNumberFormat: TitledValue.Format.Int);
			row++;

			return row;
		} // DrawVerificationData

		private static int DrawVerificationRow(
			ExcelWorksheet statSheet,
			int row,
			string title,
			decimal reference,
			string actualFormula,
			string format
		) {
			statSheet.SetCellValue(row, 1, title, true);
			statSheet.SetCellValue(row, 2, reference, sNumberFormat: format);
			statSheet.SetCellValue(row, 3, null, sNumberFormat: format);
			statSheet.Cells[row, 3].Formula = actualFormula;

			ExcelAddress rangeToApply = new ExcelAddress(row, 3, row, 3);

			var areEqual = statSheet.ConditionalFormatting.AddExpression(rangeToApply);
			areEqual.Style.Font.Color.Color = Color.DarkGreen;
			areEqual.Formula = string.Format("B{0}=C{0}", row);

			var areNotEqual = statSheet.ConditionalFormatting.AddExpression(rangeToApply);
			areNotEqual.Style.Font.Color.Color = Color.Red;
			areNotEqual.Formula = string.Format("B{0}<>C{0}", row);

			return row + 1;
		} // DrawVerificationRow

		// ReSharper disable once UnusedMethodReturnValue.Local
		private static int DrawTotalAndReject(ExcelWorksheet cfgSheet, int row, int lastRawRow) {
			AStatItem.SetBorders(cfgSheet.Cells[row, 1, row, 2]).Merge = true;
			cfgSheet.SetCellValue(row, 1, "Total and Reject data", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			cfgSheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			int totalRow = row;

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Total count", "=COUNT(Decisions!$A$2:$A{0})", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectTitle(cfgSheet, row, "Auto processed");

			int autoProcessedRow = row;

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Count", "=COUNTIFS(Decisions!$Q$2:$Q${0},\"<>Waiting\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Processed / total %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, autoProcessedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectTitle(cfgSheet, row, "Auto rejected");

			int autoRejectedRow = row;

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Count", "=COUNTIF(Decisions!$Q$2:$Q${0},\"Reject\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / total %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, autoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / Processed %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", autoProcessedRow, autoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectTitle(cfgSheet, row, "Manually rejected");

			int manuallyRejectedRow = row;

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Count", "=COUNTIF(Decisions!$J$2:$J${0},\"Rejected\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / Processed %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, manuallyRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectTitle(cfgSheet, row, "Manually and auto rejected");

			int manuallyAndAutoRejectedRow = row;

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Count", "=COUNTIFS(Decisions!$J$2:$J${0},\"Rejected\", Decisions!$Q$2:$Q${0},\"Reject\")", lastRawRow, TitledValue.Format.Int
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / total %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", totalRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / processed %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", autoProcessedRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / manually rejected %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", manuallyRejectedRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			row = DrawTotalAndRejectRow(cfgSheet, row,
				"Rejected / auto rejected %",
				string.Format("=IF(B{0}=0,0,B{1}/B{0})", autoRejectedRow, manuallyAndAutoRejectedRow),
				null,
				TitledValue.Format.Percent
			);

			return row;
		} // DrawVerificationData

		private static int DrawTotalAndRejectRow(
			ExcelWorksheet cfgSheet,
			int row,
			string title,
			string formulaPattern,
			int? lastRawRow,
			string valueFormat
		) {
			cfgSheet.SetCellValue(row, 1, title, bSetZebra: false);

			var cell = cfgSheet.Cells[row, 2];

			cell.SetCellValue(null, bSetZebra: false, sNumberFormat: valueFormat);

			cell.Formula = lastRawRow == null ? formulaPattern : string.Format(formulaPattern, lastRawRow.Value);

			return row + 1;
		} // DrawTotalAndRejectRow

		private static int DrawTotalAndRejectTitle(ExcelWorksheet cfgSheet, int row, string title) {
			var range = cfgSheet.Cells[row, 1, row, 2];

			range.Merge = true;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Bisque);
			range.Style.Font.Bold = true;
			range.Value = title;

			return row + 1;
		} // DrawTotalAndRejectTitle

		private void LoadCashRequests() {
			Data.Clear();

			var byCustomer = new SortedDictionary<int, CustomerData>();

			ProgressCounter pc = new ProgressCounter("{0} cash requests loaded so far...", Log, 50);

			SetupFeeCalculatorLegacy.ReloadBrokerRepoCache();

			this.spLoad.ForEachResult<SpLoadCashRequestsForAutomationReport.ResultRow>(sr => {
				if (byCustomer.ContainsKey(sr.CustomerID))
					byCustomer[sr.CustomerID].Add(sr);
				else
					byCustomer[sr.CustomerID] = new CustomerData(sr, this.tag, Log);

				pc++;
				return ActionResult.Continue;
			});

			pc.Log();

			Log.Debug("{0} loaded before filtering.", Grammar.Number(Data.Count, "cash request"));

			Data.Clear();

			foreach (CustomerData customerData in byCustomer.Values) {
				customerData.FindLoansAndFilterAraOut(CashRequestLoans, LoanSources);
				Data.AddRange(customerData.Data);
			} // for each

			Log.Debug("{0} remained after filtering cash requests.", Grammar.Number(Data.Count, "data item"));
		} // LoadCashRequests

		private bool IsHomeOwner(int customerID) {
			if (this.homeOwners.ContainsKey(customerID))
				return this.homeOwners[customerID];

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			this.homeOwners[customerID] = isHomeOwnerAccordingToLandRegistry;

			return isHomeOwnerAccordingToLandRegistry;
		} // IsHomeOwner

		private readonly string tag;

		private readonly SortedDictionary<int, bool> homeOwners;
		private readonly TCrLoans crLoans;
		private readonly SortedSet<string> loanSources;
		private readonly SpLoadCashRequestsForAutomationReport spLoad;

		private static class Reference {
			public static class Approve {
				public const int Count = 4621;
				public const decimal Amount = 45888566m;
			} // class Approve

			public static class Loan {
				public const int Count = 3938;
				public const decimal Amount = 37577566m;
			} // class Loan

			public static class Default {
				public const int Count = 317;

				public static class Issued {
					public const decimal Amount = 2567666m;
				} // class Issued

				public static class Outstanding {
					public const decimal Amount = 1651147.56m;
				} // class Outstanding
			} // class Default
		} // class Reference

		private void AppendDecisionTitles(ExcelWorksheet decisionSheet) {
			int column = decisionSheet.SetRowTitles(1, CsvTitles);

			foreach (string formula in decisionTitleFormulae) {
				if (formula.StartsWith("=")) {
					decisionSheet.SetCellTitle(1, column, null);
					decisionSheet.Cells[1, column].Formula = formula;
					column++;
				} else
					column = decisionSheet.SetCellTitle(1, column, formula);
			} // for each
		} // AppendDecisionTitles

		private const string DecisionsSheetName = "Decisions";

		private static readonly string[] decisionTitleFormulae = {
			"Total repaid amount",
			"Outstanding amount",
			"Approved amount",
			"Min offer: issued amount",
			"Min offer: outstanding amount",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" auto decision\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" approved amount\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" issued amount\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" outstanding amount\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" auto decision\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" approved amount\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" issued amount\")",
			"=CONCATENATE(\"Min offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" outstanding amount\")",
			"Max offer:" + System.Environment.NewLine + "approved amount",
			"Max offer:" + System.Environment.NewLine + "issued amount",
			"Max offer:" + System.Environment.NewLine + "outstanding amount",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" auto decision\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" approved amount\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" issued amount\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$12, \"£ #,##\"), \" outstanding amount\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" auto decision\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" approved amount\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" issued amount\")",
			"=CONCATENATE(\"Max offer:" + System.Environment.NewLine + "\",TEXT(Verification!$B$13, \"£ #,##\"), \" outstanding amount\")",
			"Min offer: auto decision",
			"Is home owner",
			"Home owner cap",
			"Outstanding amount",
		};

		private static readonly string minOfferFormulaeSheet = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
			"Ezbob.Backend.Strategies.AutomationVerification.KPMG.MaamMedalAndPricing_MinOffer.txt"
		)).ReadToEnd();

		private static readonly string maxOfferFormulaeSheet = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(
			"Ezbob.Backend.Strategies.AutomationVerification.KPMG.MaamMedalAndPricing_MaxOffer.txt"
		)).ReadToEnd();

		private const string LastRawRow = "__LAST_RAW_ROW__";
	} // class MaamMedalAndPricing
} // namespace

