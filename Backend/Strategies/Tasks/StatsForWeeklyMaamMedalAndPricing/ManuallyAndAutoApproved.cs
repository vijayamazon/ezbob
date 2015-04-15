namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyAndAutoApproved : AStatItem {
		public ManuallyAndAutoApproved(
			bool takeMin,
			ASafeLog log,
			ExcelWorksheet sheet,
			Total total,
			ManuallyApproved manuallyApproved,
			AutoApproved autoApproved
		) : base(
			log.Safe(),
			sheet,
			"Manually and auto approved",
			total,
			manuallyApproved,
			autoApproved
		) {
			this.takeMin = takeMin;
			this.manualAmount = 0;
			this.autoAmount = 0;

			this.manualLoanCount = new LoanCount(true, Log);
			AutoLoanCount = new LoanCount(takeMin, Log);
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(ManuallyApproved.LastWasAdded && AutoApproved.LastWasAdded)) {
				this.manualAmount += ManuallyApproved.LastAmount;
				this.autoAmount += AutoApproved.LastAmount;

				this.manualLoanCount += d.Manual(cashRequestIndex).ActualLoanCount;
				AutoLoanCount += this.takeMin
					? d.Auto(cashRequestIndex).MinLoanCount
					: d.Auto(cashRequestIndex).MaxOffer.LoanCount;
			} // if
		} // Add

		public LoanCount AutoLoanCount { get; private set; }

		[SuppressMessage("ReSharper", "RedundantAssignment")]
		public int DrawSummary(int row, out string loanCountRatio, out string loanAmountRatio, out string defaultOutstandingRatio) {
			row = DrawSummaryTitle(row);

			int column = 1;

			int approvedRow = row;

			column = SetBorders(row, column).SetCellValue("Approved", true);
			column = SetCellGbp(row, column, this.manualAmount);
			column = SetCellInt(row, column, Count);
			column = SetCellGbp(row, column, this.autoAmount);
			column = SetCellInt(row, column, Count);

			row++;
			column = 1;

			int issuedRow = row;

			column = SetBorders(row, column).SetCellValue("Issued", true);
			column = SetCellGbp(row, column, this.manualLoanCount.Total.Amount);
			column = SetCellInt(row, column, this.manualLoanCount.Total.Count);
			column = SetCellGbp(row, column, AutoLoanCount.Total.Amount);
			column = SetCellInt(row, column, AutoLoanCount.Total.Count);

			loanCountRatio = string.Format("IF(E{1}=0,0,E{0}/E{1})", issuedRow, approvedRow);
			loanAmountRatio = string.Format("IF(D{1}=0,0,D{0}/D{1})", issuedRow, approvedRow);

			row++;
			column = 1;

			int defaultIssuedRow = row;

			column = SetBorders(row, column).SetCellValue("Default issued", true);
			column = SetCellGbp(row, column, this.manualLoanCount.DefaultIssued.Amount);
			column = SetCellInt(row, column, this.manualLoanCount.DefaultIssued.Count);
			column = SetCellGbp(row, column, AutoLoanCount.DefaultIssued.Amount);
			column = SetCellInt(row, column, AutoLoanCount.DefaultIssued.Count);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default issued rate (% of loans)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "B", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "C", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "D", defaultIssuedRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "E", defaultIssuedRow, issuedRow);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default issued rate (% of approvals)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "B", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "C", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "D", defaultIssuedRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "E", defaultIssuedRow, approvedRow);

			row++;
			column = 1;

			int defaultOutstandingRow = row;

			column = SetBorders(row, column).SetCellValue("Default outstanding", true);
			column = SetCellGbp(row, column, this.manualLoanCount.DefaultOutstanding.Amount);
			column = SetCellInt(row, column, this.manualLoanCount.DefaultOutstanding.Count);
			column = SetCellGbp(row, column, AutoLoanCount.DefaultOutstanding.Amount);
			column = SetCellInt(row, column, AutoLoanCount.DefaultOutstanding.Count);

			defaultOutstandingRatio = string.Format("IF(D{1}=0,0,D{0}/D{1})", defaultOutstandingRow, defaultIssuedRow);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default outstanding rate (% of loans)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "B", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "C", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "D", defaultOutstandingRow, issuedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "E", defaultOutstandingRow, issuedRow);

			row++;
			column = 1;

			column = SetBorders(row, column).SetCellValue("Default outstanding rate (% of approvals)", true);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "B", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "C", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "D", defaultOutstandingRow, approvedRow);
			column = SetFormula(row, column, TitledValue.Format.Percent, FormulaColour, "={0}{1}/{0}{2}", "E", defaultOutstandingRow, approvedRow);

			return InsertDivider(row);
		} // DrawSummary

		protected override TitledValue[] PrepareCountRowValues() {
			return null;
		} // PrepareCountRowValues

		protected override List<TitledValue[]> PrepareMultipleCountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("count", Count),
				},
				new[] {
					new TitledValue("both approved / total %", Count, Total.Count),
					new TitledValue("both approved / manually approved %", Count, ManuallyApproved.Count),
					new TitledValue("both approved / autoApproved %", Count, AutoApproved.Count),
				},
				new[] {
					new TitledValue("loan count", this.manualLoanCount.Total.Count),
				},
				new[] {
					new TitledValue("default loan count", this.manualLoanCount.DefaultIssued.Count),
				},
			};
		} // PrepareMultipleCountRowValues

		protected override TitledValue[] PrepareAmountRowValues() {
			return null;
		} // PrepareAmountRowValues

		protected override List<TitledValue[]> PrepareMultipleAmountRowValues() {
			return new List<TitledValue[]> {
				new[] {
					new TitledValue("manual amount", this.manualAmount),
					new TitledValue("manual amount / total manual amount %", this.manualAmount, ManuallyApproved.Amount),
				},
				new[] {
					new TitledValue("auto amount", this.autoAmount),
					new TitledValue("auto amount / total auto amount %", this.autoAmount, AutoApproved.Amount),
					new TitledValue("auto amount / manual amount %", this.autoAmount, this.manualAmount),
				},
				new[] {
					new TitledValue("manual loan amount", this.manualLoanCount.Total.Amount),
					new TitledValue("manual default issued loan amount", this.manualLoanCount.DefaultIssued.Amount),
					new TitledValue("manual default outstanding loan amount", this.manualLoanCount.DefaultOutstanding.Amount),
				},
				new[] {
					new TitledValue("auto loan amount", AutoLoanCount.Total.Amount),
					new TitledValue("auto default issued loan amount", AutoLoanCount.DefaultIssued.Amount),
					new TitledValue("auto default outstanding loan amount", AutoLoanCount.DefaultOutstanding.Amount),
				},
			};
		} // PrepareMultipleAmountRowValues

		private AStatItem Total { get { return Superior[0]; } }
		private AStatItem ManuallyApproved { get { return Superior[1]; } }
		private AStatItem AutoApproved { get { return Superior[2]; } }

		private decimal manualAmount;
		private decimal autoAmount;

		private LoanCount manualLoanCount;
		private readonly bool takeMin;
	} // class ManuallyAndAutoApproved
} // namespace
