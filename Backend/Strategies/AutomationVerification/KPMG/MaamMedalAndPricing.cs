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
			this.scenarioNames = new SortedSet<string>();
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

			CreateXlsx();
		} // Execute

		public virtual ExcelPackage Xlsx { get; private set; }

		private void CreateXlsx() {
			Xlsx = new ExcelPackage();

			ExcelWorksheet verificationSheet = Xlsx.CreateSheet("Verification", false);
			ExcelWorksheet minOfferStatSheet = Xlsx.CreateSheet("Min offer", false);
			ExcelWorksheet maxOfferStatSheet = Xlsx.CreateSheet("Max offer", false);
			ExcelWorksheet decisionSheet = Xlsx.CreateSheet(DecisionsSheetName, false);
			ExcelWorksheet interestRateSheet = Xlsx.CreateSheet("Interest rate", false);
			ExcelWorksheet loanIDSheet = Xlsx.CreateSheet("Loan IDs", false);

			Datum.SetDecisionTitles(decisionSheet, this.loanSources);

			var decisionStats = new Stats(Log, minOfferStatSheet, true, minOfferFormulaeSheet, Color.Yellow);

			var stats = new List<Tuple<Stats, int>> {
				new Tuple<Stats, int>(decisionStats, -1),
				new Tuple<Stats, int>(new Stats(Log, maxOfferStatSheet, false, maxOfferFormulaeSheet, Color.LawnGreen), -1),
			};

			var pc = new ProgressCounter("{0} items sent to .xlsx", Log, 50);

			int curRow = 2;

			foreach (Datum d in Data) {
				d.ToXlsx(decisionSheet, curRow, this.scenarioNames);
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

			CreateInterestRateSheet(interestRateSheet, lastDecisionRow);

			// Currently (Apr 21 2015) the last column is CG in Decisions, so 128 should be good enough.
			Xlsx.AutoFitColumns(128);
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
				string.Format("=COUNTIF(Decisions!$J$2:$J${0},\"Approved\") ", lastRawRow),
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
				"Count",
				"=COUNTIFS(Decisions!$J$2:$J${0},\"Rejected\", Decisions!$Q$2:$Q${0},\"Reject\")",
				lastRawRow,
				TitledValue.Format.Int
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

		private void CreateInterestRateSheet(ExcelWorksheet interestRateSheet, int lastRawRow) {
			int row = 1;

			foreach (var cfg in interestRateSheetSections)
				row = CreateInterestRateSheetSection(interestRateSheet, row, lastRawRow, cfg);
		} // CreateInterestRateSheet

		private class InterestRateSheetSectionConfiguration {
			public InterestRateSheetSectionConfiguration(
				string title,
				string loanSourceColumn,
				string loanSourceCondition,
				string loanSourceEscapedCondition
			) {
				Title = title;
				LoanSourceColumn = loanSourceColumn;
				LoanSourceCondition = loanSourceCondition;
				LoanSourceEscapedCondition = loanSourceEscapedCondition;

				WeightColumnNames = new SortedDictionary<string, string> {
					{ ManualDecisionType,   "K" },
					{ MinOfferDecisionType, "BE" },
					{ MaxOfferDecisionType, "BP" },
				};

				ScenarioColumnNames = new SortedDictionary<string, string> {
					{ ManualDecisionType,   "CE" },
					{ MinOfferDecisionType, "CE" },
					{ MaxOfferDecisionType, "CF" },
				};

				DataColumnNames = new SortedDictionary<string, SortedDictionary<string, string>>();

				DataColumnNames[ManualDecisionType] = new SortedDictionary<string, string> {
					{ SetupFeeAmount, "O" },
					{ SetupFeeRate, "N" },
					{ InterestRate, "L" },
				};

				DataColumnNames[MinOfferDecisionType] = new SortedDictionary<string, string> {
					{ SetupFeeAmount, "AC" },
					{ SetupFeeRate, "AB" },
					{ InterestRate, "Z" },
				};

				DataColumnNames[MaxOfferDecisionType] = new SortedDictionary<string, string> {
					{ SetupFeeAmount, "AK" },
					{ SetupFeeRate, "AJ" },
					{ InterestRate, "AH" },
				};

				Formats = new SortedDictionary<string, string> {
					{ SetupFeeAmount, TitledValue.Format.Money },
					{ SetupFeeRate, TitledValue.Format.Percent },
					{ InterestRate, TitledValue.Format.Percent },
				};
			} // constructor

			public string Title { get; private set; }
			public string LoanSourceColumn { get; private set; }
			public string LoanSourceCondition { get; private set; }
			public string LoanSourceEscapedCondition { get; private set; }

			public SortedDictionary<string, SortedDictionary<string, string>> DataColumnNames { get; private set; }

			public SortedDictionary<string, string> WeightColumnNames { get; private set; }

			public SortedDictionary<string, string> Formats { get; private set; }

			public SortedDictionary<string, string> ScenarioColumnNames { get; private set; }
		} // class InterestRateSheetSectionConfiguration

		private static readonly List<InterestRateSheetSectionConfiguration> interestRateSheetSections =
			new List<InterestRateSheetSectionConfiguration> {
				new InterestRateSheetSectionConfiguration("All decisions", "CG", "<>\"\"", "<>\"\"\"\""),
				new InterestRateSheetSectionConfiguration("EU & COSME loans", "CH", "=\"EU\"", "EU"),
				new InterestRateSheetSectionConfiguration("COSME only", "CG", "=\"COSME\"", "COSME"),
			};

		private int CreateInterestRateSheetSection(
			ExcelWorksheet interestRateSheet,
			int row,
			int lastRawRow,
			InterestRateSheetSectionConfiguration cfg
		) {
			ExcelRange range = AStatItem.SetBorders(interestRateSheet.Cells[row, 1]);
			range.SetCellValue(cfg.Title, bSetZebra: false);
			range.Style.Font.Size = 16;
			range.Style.Font.Bold = true;
			range.Style.Fill.PatternType = ExcelFillStyle.Solid;
			range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);

			int decisionTypeWidth = interestRateSheetColumns.Count;

			int column = 2;

			foreach (string decisionType in interestRateSheetDecisionTypes) {
				range = AStatItem.SetBorders(interestRateSheet.Cells[row, column, row, column + decisionTypeWidth - 1]);
				range.Merge = true;
				range.SetCellValue(decisionType, bSetZebra: false);
				range.Style.Font.Size = 16;
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
				range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

				column += decisionTypeWidth;
			} // for each

			row++;

			row = CreateInterestRateSheetSectionGroup(interestRateSheet, row, lastRawRow, SetupFeeRate, cfg);
			row = CreateInterestRateSheetSectionGroup(interestRateSheet, row, lastRawRow, SetupFeeAmount, cfg);
			row = CreateInterestRateSheetSectionGroup(interestRateSheet, row, lastRawRow, InterestRate, cfg);

			return row;
		} // CreateInterestRateSheetSection

		private const string SetupFeeRate = "Setup fee rate";
		private const string SetupFeeAmount = "Setup fee amount";
		private const string InterestRate = "Interest rate";

		private int CreateInterestRateSheetSectionGroup(
			ExcelWorksheet interestRateSheet,
			int row,
			int lastRawRow,
			string title,
			InterestRateSheetSectionConfiguration cfg
		) {
			int column = 1;

			ExcelRange range = AStatItem.SetBorders(interestRateSheet.Cells[row, column]);

			column = range.SetCellValue(title, bIsBold: true, bSetZebra: false, oBgColour: Color.Bisque);
			range.Style.Font.Size = 13;

			// ReSharper disable once UnusedVariable
			foreach (string justForLoop in interestRateSheetDecisionTypes) {
				bool first = true;

				foreach (string colName in interestRateSheetColumns) {
					range = AStatItem.SetBorders(interestRateSheet.Cells[row, column]);

					column = range.SetCellValue(colName, bIsBold: true, bSetZebra: false, oBgColour: Color.Bisque);
					range.Style.Font.Size = 13;

					if (first)
						range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

					first = false;
				} // for each column
			} // for each decision type

			row++;

			foreach (string scenarioName in scenarioNames)
				row = CreateInterestRateSheetSectionRow(interestRateSheet, row, lastRawRow, title, scenarioName, cfg);

			return row;
		} // CreateInterestRateSheetSectionGroup

		private int CreateInterestRateSheetSectionRow(
			ExcelWorksheet interestRateSheet,
			int row,
			int lastRawRow,
			string dataFieldName,
			string scenarioName,
			InterestRateSheetSectionConfiguration cfg
		) {
			int column = 1;

			ExcelRange range = AStatItem.SetBorders(interestRateSheet.Cells[row, column]);

			column = range.SetCellValue(scenarioName, bIsBold: true, bSetZebra: false);

			foreach (string decisionType in interestRateSheetDecisionTypes) {
				bool first = true;

				foreach (string colName in interestRateSheetColumns) {
					range = AStatItem.SetBorders(interestRateSheet.Cells[row, column]);

					if (first)
						range.Style.Border.Left.Style = ExcelBorderStyle.Thick;

					first = false;

					switch (colName) {
					case CountInterestColumn:
						CreateCellCount(range, row, lastRawRow, decisionType, cfg);
						break;

					case AvgInterestColumn:
						CreateCellAvg(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;

					case WeightAvgInterestColumn:
						CreateCellWeightAvg(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;

					case MinInterestColumn:
						CreateCellMin(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;

					case MaxInterestColumn:
						CreateCellMax(range, row, lastRawRow, dataFieldName, decisionType, cfg);
						break;
					} // switch

					column++;
				} // for each column
			} // for each decision type

			return row + 1;
		} // CreateInterestRateSheetSectionRow

		private void CreateCellCount(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.Formula = string.Format(
				"=COUNTIFS({0}!${1}$2:${1}${2}, $A${3}, {0}!${4}$2:${4}${2}, \"{5}\")",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceEscapedCondition
			);

			cell.Style.Numberformat.Format = TitledValue.Format.Int;
		} // CreateCellCount

		private void CreateCellAvg(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.Formula = string.Format(
				"=AVERAGEIFS({0}!${6}$2:${6}${2}, {0}!${1}$2:${1}${2}, $A${3}, {0}!${4}$2:${4}${2}, \"{5}\")",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceEscapedCondition,
				cfg.DataColumnNames[decisionType][dataFieldName]
			);

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellAvg

		private void CreateCellWeightAvg(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.Formula = string.Format(
				"=SUMPRODUCT(" +
					"({0}!${7}$2:${7}${2} * {0}!${8}$2:${8}${2}) * " +
					"({0}!${1}$2:${1}${2} = $A${3}) * " +
					"({0}!${4}$2:${4}${2}{5})" +
				") / SUMIFS(" +
					"{0}!${8}$2:${8}${2}," +
					"{0}!${1}$2:${1}${2}, $A${3}," +
					"{0}!${4}$2:${4}${2}, \"{6}\"" +
				")",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceCondition,
				cfg.LoanSourceEscapedCondition,
				cfg.DataColumnNames[decisionType][dataFieldName],
				cfg.WeightColumnNames[decisionType]
			);

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellWeightAvg

		private void CreateCellMin(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.CreateArrayFormula(string.Format(
				"=MIN(IF({0}!${1}$2:${1}${2}=$A${3},IF({0}!${4}$2:${4}${2}{5},{0}!${6}$2:${6}${2})))",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceCondition,
				cfg.DataColumnNames[decisionType][dataFieldName]
			));

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellWeightMin

		private void CreateCellMax(
			ExcelRange cell,
			int row,
			int lastRawRow,
			string dataFieldName,
			string decisionType,
			InterestRateSheetSectionConfiguration cfg
		) {
			cell.CreateArrayFormula(string.Format(
				"=MAX(IF({0}!${1}$2:${1}${2}=$A${3},IF({0}!${4}$2:${4}${2}{5},{0}!${6}$2:${6}${2})))",
				DecisionsSheetName,
				cfg.ScenarioColumnNames[decisionType],
				lastRawRow,
				row,
				cfg.LoanSourceColumn,
				cfg.LoanSourceCondition,
				cfg.DataColumnNames[decisionType][dataFieldName]
			));

			cell.Style.Numberformat.Format = cfg.Formats[dataFieldName];
		} // CreateCellWeightMax

		private const string CountInterestColumn = "Count";
		private const string AvgInterestColumn = "Simple average";
		private const string WeightAvgInterestColumn = "Weighted average"; 
		private const string MinInterestColumn = "Minimum"; 
		private const string MaxInterestColumn = "Maximum";

		private static readonly List<string> interestRateSheetColumns = new List<string> {
			CountInterestColumn, AvgInterestColumn, WeightAvgInterestColumn, MinInterestColumn, MaxInterestColumn,
		};

		private const string ManualDecisionType = "Manual";
		private const string MinOfferDecisionType ="Min offer";
		private const string MaxOfferDecisionType ="Max offer";

		private static readonly List<string> interestRateSheetDecisionTypes = new List<string> {
			ManualDecisionType, MinOfferDecisionType, MaxOfferDecisionType,
		};

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
					public const decimal Amount = 2572666m;
				} // class Issued

				public static class Outstanding {
					public const decimal Amount = 1651147.56m;
				} // class Outstanding
			} // class Default
		} // class Reference

		private readonly SortedSet<string> scenarioNames; 

		private const string DecisionsSheetName = "Decisions";

		// ReSharper disable AssignNullToNotNullAttribute

		private static readonly string minOfferFormulaeSheet = new StreamReader(
			Assembly.GetExecutingAssembly().GetManifestResourceStream(
				"Ezbob.Backend.Strategies.AutomationVerification.KPMG.MaamMedalAndPricing_MinOffer.txt"
			)
		).ReadToEnd();

		private static readonly string maxOfferFormulaeSheet = new StreamReader(
			Assembly.GetExecutingAssembly().GetManifestResourceStream(
				"Ezbob.Backend.Strategies.AutomationVerification.KPMG.MaamMedalAndPricing_MaxOffer.txt"
			)
		).ReadToEnd();

		// ReSharper restore AssignNullToNotNullAttribute
	} // class MaamMedalAndPricing
} // namespace

