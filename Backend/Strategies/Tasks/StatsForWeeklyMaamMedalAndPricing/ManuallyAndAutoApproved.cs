﻿namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyAndAutoApproved : AStatItem {
		public ManuallyAndAutoApproved(
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
			this.manualAmount = 0;
			this.autoAmount = 0;

			this.manualLoanCount = new LoanCount(Log);
			AutoLoanCount = new LoanCount(Log);
		} // constructor

		public override void Add(Datum d, int cashRequestIndex) {
			if (Added.If(ManuallyApproved.LastWasAdded && AutoApproved.LastWasAdded)) {
				this.manualAmount += ManuallyApproved.LastAmount;
				this.autoAmount += AutoApproved.LastAmount;

				this.manualLoanCount += d.Manual(cashRequestIndex).LoanCount;
				AutoLoanCount += d.Auto(cashRequestIndex).LoanCount;
			} // if
		} // Add

		public LoanCount AutoLoanCount { get; private set; }

		public int DrawSummary(int row) {
			row = SetRowValues(row, true,
				new TitledValue("Manual amount", "Manual count"),
				new TitledValue("Auto amount", "Auto count")
			);

			row = SetRowValues(row, "Approved",
				new TitledValue(this.manualAmount, Count),
				new TitledValue(this.autoAmount, Count)
			);

			row = SetRowValues(row, "Issued",
				new TitledValue(this.manualLoanCount.Total.Amount, this.manualLoanCount.Total.Count),
				new TitledValue(AutoLoanCount.Total.Amount, AutoLoanCount.Total.Count)
			);

			row = SetRowValues(row, "Default issued",
				new TitledValue(this.manualLoanCount.DefaultIssued.Amount, this.manualLoanCount.DefaultIssued.Count),
				new TitledValue(AutoLoanCount.DefaultIssued.Amount, AutoLoanCount.DefaultIssued.Count)
			);

			row = SetRowValues(row, "Default issued rate (% of loans)",
				new TitledValue(
					this.manualLoanCount.DefaultIssued.Amount, this.manualLoanCount.Total.Amount,
					AutoLoanCount.DefaultIssued.Count, AutoLoanCount.Total.Count
				),
				new TitledValue(
					AutoLoanCount.DefaultIssued.Amount, AutoLoanCount.Total.Amount,
					AutoLoanCount.DefaultIssued.Count, AutoLoanCount.Total.Count
				)
			);

			row = SetRowValues(row, "Default issued rate (% of approvals)",
				new TitledValue(
					this.manualLoanCount.DefaultIssued.Amount, this.manualAmount,
					this.manualLoanCount.DefaultIssued.Count, Count
				),
				new TitledValue(
					this.manualLoanCount.DefaultIssued.Amount, this.autoAmount,
					AutoLoanCount.DefaultIssued.Count, Count
				)
			);

			row = SetRowValues(row, "Default outstanding",
				new TitledValue(this.manualLoanCount.DefaultOutstanding.Amount, this.manualLoanCount.DefaultOutstanding.Count),
				new TitledValue(AutoLoanCount.DefaultOutstanding.Amount, AutoLoanCount.DefaultOutstanding.Count)
			);

			row = SetRowValues(row, "Default outstanding rate (% of loans)",
				new TitledValue(
					this.manualLoanCount.DefaultOutstanding.Amount, this.manualLoanCount.Total.Amount,
					AutoLoanCount.DefaultOutstanding.Count, AutoLoanCount.Total.Count
				),
				new TitledValue(
					AutoLoanCount.DefaultOutstanding.Amount, AutoLoanCount.Total.Amount,
					AutoLoanCount.DefaultOutstanding.Count, AutoLoanCount.Total.Count
				)
			);

			row = SetRowValues(row, "Default outstanding rate (% of approvals)",
				new TitledValue(
					this.manualLoanCount.DefaultOutstanding.Amount, this.manualAmount,
					this.manualLoanCount.DefaultOutstanding.Count, Count
				),
				new TitledValue(
					this.manualLoanCount.DefaultOutstanding.Amount, this.autoAmount,
					AutoLoanCount.DefaultOutstanding.Count, Count
				)
			);

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
	} // class ManuallyAndAutoApproved
} // namespace
