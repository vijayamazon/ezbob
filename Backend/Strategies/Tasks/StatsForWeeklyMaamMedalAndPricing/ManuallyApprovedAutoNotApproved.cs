namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyApprovedAutoNotApproved : ARejectedCrossApproved {
		public ManuallyApprovedAutoNotApproved(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			takeMin,
			log.Safe(),
			sheet,
			title,
			total,
			manuallyApproved,
			autoApproved
		) {} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(ManuallyApproved.LastWasAdded && !AutoApproved.LastWasAdded, ManuallyApproved.LastAmount))
				LoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
		} // Add

		[SuppressMessage("ReSharper", "RedundantAssignment")]
		public override int DrawSummary(int row) {
			row = DrawSummaryTitle(row);

			int column = 1;

			int approvedRow = row;

			column = SetBorders(row, column).SetCellValue("Approved", true);
			column = SetCellGbp(row, column, Amount);
			column = SetCellInt(row, column, Count);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Average approved amount", true);
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=IF(C{0}=0,0,B{0}/C{0})", approvedRow);
			column = SetBorders(row, column).SetCellValue("");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			int issuedRow = row;

			column = SetBorders(row, column).SetCellValue("Issued", true);
			column = SetCellGbp(row, column, LoanCount.Total.Amount);
			column = SetCellInt(row, column, LoanCount.Total.Count);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Average issued amount", true);
			column = SetFormula(row, column, TitledValue.Format.Money, FormulaColour, "=IF(C{0}=0,0,B{0}/C{0})", issuedRow);
			column = SetBorders(row, column).SetCellValue("");
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			int defaultIssuedRow = row;

			column = SetBorders(row, column).SetCellValue("Default issued", true);
			column = SetCellGbp(row, column, LoanCount.DefaultIssued.Amount);
			column = SetCellInt(row, column, LoanCount.DefaultIssued.Count);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default issued rate (% of loans)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=B{0}/B{1}", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=C{0}/C{1}", defaultIssuedRow, issuedRow);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default issued rate (% of approvals)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=B{0}/B{1}", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=C{0}/C{1}", defaultIssuedRow, approvedRow);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			int defaultOutstandingRow = row;

			column = SetBorders(row, column).SetCellValue("Default outstanding", true);
			column = SetCellGbp(row, column, LoanCount.DefaultOutstanding.Amount);
			column = SetCellInt(row, column, LoanCount.DefaultOutstanding.Count);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default outstanding rate (% of loans)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=B{0}/B{1}", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=C{0}/C{1}", defaultOutstandingRow, issuedRow);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default outstanding rate (% of approvals)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=B{0}/B{1}", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "=C{0}/C{1}", defaultOutstandingRow, approvedRow);
			column = SetBorders(row, column).SetCellValue("N/A");
			column = SetBorders(row, column).SetCellValue("N/A");

			return InsertDivider(row);
		} // DrawSummary

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new [] {
					new TitledValue("count", Count),
				},
				new [] {
					new TitledValue("count / total %", Count, Total.Count),
					new TitledValue("count / not auto approved %", Count, Total.Count - AutoApproved.Count),
					new TitledValue("count / manually approved %", Count, ManuallyApproved.Count),
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
					new TitledValue("amount / manually approved %", Amount, ManuallyApproved.Amount),
				},
				new [] {
					new TitledValue("loan amount", LoanCount.Total.Amount),
					new TitledValue("default issued loan amount", LoanCount.DefaultIssued.Amount),
					new TitledValue("default outstanding loan amount", LoanCount.DefaultOutstanding.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private ManuallyApproved ManuallyApproved { get { return (ManuallyApproved)Superior[1]; } }
		private AutoApproved AutoApproved { get { return (AutoApproved)Superior[2]; } }
	} // class ManuallyApprovedAutoNotApproved
} // namespace
