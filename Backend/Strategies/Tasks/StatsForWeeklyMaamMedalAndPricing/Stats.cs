namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using OfficeOpenXml;

	internal class Stats {
		public Stats(ASafeLog log, ExcelWorksheet sheet, bool takeMin, string cashRequestSourceName) {
			this.sheet = sheet;

			log = log.Safe();

			var total = new Total(log, sheet);
			var autoProcessed = new AutoProcessed(log, sheet, total);
			var autoRejected = new AutoRejected(log, sheet, total, autoProcessed);
			var autoApproved = new AutoApproved(log, takeMin, sheet, total, autoProcessed);
			var manuallyRejected = new ManuallyRejected(log, sheet, total);
			ManuallyApproved = new ManuallyApproved(log, sheet, total);

			this.manuallyAndAutoApproved = new ManuallyAndAutoApproved(takeMin, log, sheet, total, ManuallyApproved, autoApproved);

			this.manuallyRejectedAutoApproved = new ManuallyRejectedAutoApproved(takeMin, log, sheet, "Manually rejected and auto approved", total, manuallyRejected, autoApproved);
			this.manuallyApprovedAutoNotApproved = new ManuallyApprovedAutoNotApproved(takeMin, log, sheet, "Manually approved and auto NOT approved", total, ManuallyApproved, autoApproved);

			this.stats = new List<AStatItem> {
				total,
				autoProcessed,
				autoRejected,
				autoApproved,
				manuallyRejected,
				new ManuallyAndAutoRejected(log, sheet, total, manuallyRejected, autoRejected),
				ManuallyApproved,
				new DefaultIssuedLoans(log, sheet, total, ManuallyApproved, autoApproved),
				new DefaultOutstandingLoans(log, sheet, total, ManuallyApproved, autoApproved),
				this.manuallyAndAutoApproved,
				this.manuallyRejectedAutoApproved,
				this.manuallyApprovedAutoNotApproved,
			};

			this.name = (takeMin ? "Minimum" : "Maximum") + " offer " + cashRequestSourceName;
		} // constructor

		public void Add(Datum d, int cashRequstIndex) {
			foreach (AStatItem si in this.stats)
				si.Add(d, cashRequstIndex);
		} // Add

		// ReSharper disable once UnusedMethodReturnValue.Local
		public int ToXlsx(int row) {
			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, this.name, bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Summary", bSetZebra: false, oBgColour: Color.Bisque, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 14;
			row++;

			/*

			int rowMAAA = row + 1;

			string loanCountRatio;
			string loanAmountRatio;
			string defaultOutstandingRatio;

			row = this.manuallyAndAutoApproved.DrawSummary(
				rowMAAA,
				out loanCountRatio,
				out loanAmountRatio,
				out defaultOutstandingRatio
			);

			this.manuallyRejectedAutoApproved.LoanCountRatio = loanCountRatio;
			this.manuallyRejectedAutoApproved.LoanAmountRatio = loanAmountRatio;
			this.manuallyRejectedAutoApproved.DefaultOutstandingRatio = defaultOutstandingRatio;

			int rowMRAA = row + 1;

			row = this.manuallyRejectedAutoApproved.DrawSummary(rowMRAA);

			int rowMAAR = row + 1;

			row = this.manuallyApprovedAutoNotApproved.DrawSummary(rowMAAR);

			row = DrawTotalSummary(row + 1, rowMAAA, rowMRAA, rowMAAR);
			*/

			AStatItem.SetBorders(this.sheet.Cells[row, 1, row, AStatItem.LastColumnNumber]).Merge = true;
			this.sheet.SetCellValue(row, 1, "Details", bSetZebra: false, oBgColour: Color.Coral, bIsBold: true);
			this.sheet.Cells[row, 1].Style.Font.Size = 14;
			row++;

			foreach (var si in this.stats)
				row = si.ToXlsx(row + 1);

			for (int i = 1; i <= AStatItem.LastColumnNumber; i++)
				this.sheet.Column(i).AutoFit();

			return row;
		} // ToXlsx

		public int FlushLoanIDs(ExcelWorksheet targetSheet, int column) {
			column = FlushLoanIDList(targetSheet, column, "all loans", ManuallyApproved.LoanCount.IDs);

			return FlushLoanIDList(targetSheet, column, "default loans", ManuallyApproved.LoanCount.DefaultIDs);
		} // FlushLoanIDs

		public ManuallyApproved ManuallyApproved { get; private set; }

		[SuppressMessage("ReSharper", "RedundantAssignment")]
		private int DrawTotalSummary(int row, int rowMAAA, int rowMRAA, int rowMAAR) {
			const int countOffset = 1;
			const int issuedOffset = countOffset + 2;
			const int defaultIssuedOffset = issuedOffset + 3;
			const int defaultOutstandingOffset = defaultIssuedOffset + 3;

			int offset;

			int column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("TOTAL", true);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Manual amount", true);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Manual count", true);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Auto amount", true);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Auto count", true);

			row++;
			column = 1;

			int approvedRow = row;
			offset = countOffset;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Approved", true);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "B", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "C", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "D", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "E", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Average approved amount", true);
			column = SetFormula(row, column, TitledValue.Format.Money, AStatItem.FormulaColour, "=IF(C{0}=0,0,B{0}/C{0})", approvedRow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("");
			column = SetFormula(row, column, TitledValue.Format.Money, AStatItem.FormulaColour, "=IF(E{0}=0,0,D{0}/E{0})", approvedRow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("");

			row++;
			column = 1;

			int issuedRow = row;
			offset = issuedOffset;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Issued", true);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "B", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "C", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "D", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "E", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Average issued amount", true);
			column = SetFormula(row, column, TitledValue.Format.Money, AStatItem.FormulaColour, "=IF(C{0}=0,0,B{0}/C{0})", issuedRow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("");
			column = SetFormula(row, column, TitledValue.Format.Money, AStatItem.FormulaColour, "=IF(E{0}=0,0,D{0}/E{0})", issuedRow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("");

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Issued amount / approved amount", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, AStatItem.FormulaColour, "=IF(B{0}=0,0,B{1}/B{0})", approvedRow, issuedRow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("");
			column = SetFormula(row, column, TitledValue.Format.Percent, AStatItem.FormulaColour, "=IF(D{0}=0,0,D{1}/D{0})", approvedRow, issuedRow);
			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("");

			row++;
			column = 1;

			int defaultIssuedRow = row;
			offset = defaultIssuedOffset;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Default issued", true);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "B", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "C", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "D", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "E", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Default issued rate (% of loans)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "B", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "C", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "D", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "E", defaultIssuedRow, issuedRow);

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Default issued rate (% of approvals)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "B", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "C", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "D", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "E", defaultIssuedRow, approvedRow);

			row++;
			column = 1;

			int defaultOutstandingRow = row;
			offset = defaultOutstandingOffset;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Default outstanding", true);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "B", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "C", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Money, ThreeSum, "D", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);
			column = SetFormula(row, column, TitledValue.Format.Int,   ThreeSum, "E", rowMAAA + offset, rowMRAA + offset, rowMAAR + offset);

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Default outstanding rate (% of loans)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "B", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "C", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "D", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "E", defaultOutstandingRow, issuedRow);

			row++;
			column = 1;

			column = AStatItem.SetBorders(this.sheet.Cells[row, column]).SetCellValue("Default outstanding rate (% of approvals)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "B", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "C", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "D", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, "={0}{1}/{0}{2}", "E", defaultOutstandingRow, approvedRow);

			return row + 2;
		} // DrawTotalSummary

		[StringFormatMethod("formulaFormat")]
		private int SetFormula(int row, int column, string valueFormat, string formulaFormat, params object[] args) {
			return AStatItem.SetFormula(this.sheet, row, column, valueFormat, formulaFormat, args);
		} // SetFormula

		[StringFormatMethod("formulaFormat")]
		private int SetFormula(int row, int column, string valueFormat, Color fontColor, string formulaFormat, params object[] args) {
			return AStatItem.SetFormula(this.sheet, row, column, valueFormat, fontColor, formulaFormat, args);
		} // SetFormula

		private int FlushLoanIDList(ExcelWorksheet targetSheet, int column, string title, IEnumerable<int> ids) {
			targetSheet.SetCellValue(1, column, this.name + " - " + title);

			int row = 2;
			foreach (int id in ids) {
				targetSheet.SetCellValue(row, column, id);
				row++;
			} // for each
			
			return column + 1;
		} // FlushLoanIDList

		private readonly string name;

		private readonly List<AStatItem> stats; 

		private readonly ExcelWorksheet sheet;

		private readonly ManuallyAndAutoApproved manuallyAndAutoApproved;
		private readonly ManuallyRejectedAutoApproved manuallyRejectedAutoApproved;
		private readonly ManuallyApprovedAutoNotApproved manuallyApprovedAutoNotApproved;

		private static string GetOneTerm(int x) {
			return string.Format("IF({{0}}{{{0}}}=\"N/A\",0,{{0}}{{{0}}})", x);
		} // GetOneTerm

		private static readonly string ThreeSum = string.Format("={0}+{1}+{2}", GetOneTerm(1), GetOneTerm(2), GetOneTerm(3));
	} // class Stats
} // namespace
