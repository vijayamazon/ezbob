namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using OfficeOpenXml;

	internal class ExcelGenerator {
		public ExcelGenerator(
			List<Datum> data,
			SortedSet<string> loanSources,
			SortedDictionary<long, AutomationTrails> automationTrails
		) {
			this.scenarioNames = new SortedSet<string>();

			this.data = data;
			this.loanSources = loanSources;
			this.automationTrails = automationTrails;

			this.autoDecisionNames = new SortedSet<string>();
			this.manualDecisionNames = new SortedSet<string>();

			Xlsx = new ExcelPackage();
		} // constructor

		public virtual ExcelPackage Xlsx { get; private set; }

		public void Run() {
			// The lines below actually create a sheets in the output workbook in order of appearance.

			var verificationSheet = new SheetVerification(Xlsx);
			ExcelWorksheet minOfferStatSheet = Xlsx.CreateSheet("Min offer", false);
			ExcelWorksheet maxOfferStatSheet = Xlsx.CreateSheet("Max offer", false);
			ExcelWorksheet decisionSheet = Xlsx.CreateSheet(DecisionsSheetName, false);
			var interestRateSheet = new SheetInterestRate(Xlsx, this.scenarioNames);
			var automationTrailsSheet = new SheetAutomationTrails(Xlsx, this.automationTrails);
			var dddSheet = new SheetDDD(Xlsx, DecisionsSheetName, this.autoDecisionNames, this.manualDecisionNames);
			ExcelWorksheet loanIDSheet = Xlsx.CreateSheet("Loan IDs", false);

			// At this point all the sheets have been created.

			Datum.SetDecisionTitles(decisionSheet, this.loanSources);

			var decisionStats = new Stats(Log, minOfferStatSheet, true, minOfferFormulaeSheet, Color.Yellow);

			var stats = new List<Tuple<Stats, int>> {
				new Tuple<Stats, int>(decisionStats, -1),
				new Tuple<Stats, int>(new Stats(Log, maxOfferStatSheet, false, maxOfferFormulaeSheet, Color.LawnGreen), -1),
			};

			var pc = new ProgressCounter("{0} items sent to .xlsx", Log, 50);

			int curRow = 2;

			foreach (Datum d in this.data) {
				d.ToXlsx(decisionSheet, curRow, this.scenarioNames);

				automationTrailsSheet.Generate(d.Manual(-1).CashRequestID);

				this.autoDecisionNames.Add(d.Auto(-1).AutomationDecision.ToString());
				this.manualDecisionNames.Add(d.Manual(-1).DecisionStr);

				curRow++;

				foreach (Tuple<Stats, int> pair in stats)
					pair.Item1.Add(d, pair.Item2);

				pc++;
			} // for each

			pc.Log();

			int lastDecisionRow = curRow - 1;

			verificationSheet.Generate(lastDecisionRow);

			int loanIDColumn = 1;

			foreach (Tuple<Stats, int> pair in stats) {
				pair.Item1.ToXlsx(1, lastDecisionRow);

				loanIDColumn = pair.Item1.FlushLoanIDs(loanIDSheet, loanIDColumn);
			} // for each

			interestRateSheet.Create(lastDecisionRow);

			dddSheet.Generate(lastDecisionRow);

			// Currently (Apr 21 2015) the last column is CG in Decisions, so 128 should be good enough.
			Xlsx.AutoFitColumns(128);
		} // Run

		private static ASafeLog Log { get { return Library.Instance.Log; } } // Log

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

		private readonly List<Datum> data;
		private readonly SortedSet<string> loanSources;
		private readonly SortedDictionary<long, AutomationTrails> automationTrails;
		private readonly SortedSet<string> autoDecisionNames;
		private readonly SortedSet<string> manualDecisionNames;
	} // class ExcelGenerator
} // namespace
