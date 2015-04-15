namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyRejectedAutoApproved : ARejectedCrossApproved {
		public ManuallyRejectedAutoApproved(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			ManuallyRejected rejected,
			AutoApproved approved
		) : base(
			takeMin,
			log.Safe(),
			sheet,
			title,
			total,
			rejected,
			approved
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(Rejected.LastWasAdded && Approved.LastWasAdded, Approved.LastAmount))
				LoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
		} // Add

		public string LoanCountRatio { get; set; }
		public string LoanAmountRatio { get; set; }
		public string DefaultOutstandingRatio { get; set; }

		[SuppressMessage("ReSharper", "RedundantAssignment")]
		public override int DrawSummary(int row) {
			row = DrawSummaryTitle(row);

			int column = 1;

			int approvedRow = row;

			column = SetBorders(row, column).SetCellValue("Approved", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetCellGbp(row, column, Amount);
			column = SetCellInt(row, column, Count);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Average approved amount", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=IF(E{0}=0,0,D{0}/E{0})", approvedRow);
			column = SetBorders(row, column).SetCellValue("");

			row++;
			column = 1;

			int issuedRow = row;

			column = SetBorders(row, column).SetCellValue("Issued", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=D{0} * {1}", approvedRow, LoanAmountRatio);
			column = SetFormula(row, column, TitledValue.Format.Int,   FormulaColour, "=E{0} * {1}", approvedRow, LoanCountRatio);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Average issued amount", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=IF(E{0}=0,0,D{0}/E{0})", issuedRow);
			column = SetBorders(row, column).SetCellValue("");

			row++;
			column = 1;

			int defaultIssuedRow = row;
			int defaultIssuedRateRow = row + 1;

			column = SetBorders(row, column).SetCellValue("Default issued", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=D{0} * D{1}", issuedRow, defaultIssuedRateRow);
			column = SetFormula(row, column, TitledValue.Format.Int,   FormulaColour, "=E{0} * E{1}", issuedRow, defaultIssuedRateRow);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default issued rate (% of loans)", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue(0.2m, oFontColour: InputFieldColour, sNumberFormat: TitledValue.Format.Percent);
			column = SetBorders(row, column).SetCellValue(0.2m, oFontColour: InputFieldColour, sNumberFormat: TitledValue.Format.Percent);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default outstanding", true);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=D{0} * {1}", defaultIssuedRow, DefaultOutstandingRatio);
			column = SetFormula(row, column, TitledValue.Format.Int,   FormulaColour, "=E{0}", defaultIssuedRow);

			row++;

			return InsertDivider(row);
		} // DrawSummary

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new [] {
					new TitledValue("count", Count),
				},
				new [] {
					new TitledValue("count / total %", Count, Total.Count),
					new TitledValue("count / rejected %", Count, Rejected.Count),
					new TitledValue("count / approved %", Count, Approved.Count),
				},
				new [] {
					new TitledValue("loan count", LoanCount.Total.Count),
				},
				new [] {
					new TitledValue("default loan count", LoanCount.DefaultIssued.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		protected override List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("amount", Amount),
					new TitledValue("amount / approved %", Amount, Approved.Amount),
				},
				new [] {
					new TitledValue("loan amount", LoanCount.Total.Amount),
					new TitledValue("default issued loan amount", LoanCount.DefaultIssued.Amount),
					new TitledValue("default outstanding loan amount", LoanCount.DefaultOutstanding.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private ManuallyRejected Rejected { get { return (ManuallyRejected)Superior[1]; } }
		private AutoApproved Approved { get { return (AutoApproved)Superior[2]; } }
	} // class ManuallyRejectedAutoApproved
} // namespace
