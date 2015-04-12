namespace Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing {
	using System;
	using System.Drawing.Design;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal class ManuallyRejectedAutoApproved : ARejectedCrossApproved {
		public ManuallyRejectedAutoApproved(
			ASafeLog log,
			ExcelWorksheet sheet,
			string title,
			Total total,
			AStatItem rejected,
			AStatItem approved
		) : base(
			log.Safe(),
			sheet,
			title,
			total,
			rejected,
			approved
		) {} // constructor

		/// <summary>
		/// Issued loan count / approved count.
		/// </summary>
		public decimal IssuedCountRate { get; set; }

		/// <summary>
		/// Default outstanding amount / default issued amount.
		/// </summary>
		public decimal OutstandingAmountRate { get; set; }

		public override int DrawSummary(int row) {
			const decimal defaultIssuedRate = 0.2m;

			int firstRow = row;

			int column = 1;

			column = this.sheet.SetCellValue(row, column, "Manually rejected and auto approved", true);
			column = this.sheet.SetCellValue(row, column, "Manual amount", true);
			column = this.sheet.SetCellValue(row, column, "Manual count", true);
			column = this.sheet.SetCellValue(row, column, "Auto amount", true);
			column = this.sheet.SetCellValue(row, column, "Auto count", true);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Approved", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Count, sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Issued", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount * IssuedCountRate, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Math.Round(Count * IssuedCountRate), sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default issued", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount * IssuedCountRate * defaultIssuedRate, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Math.Round(Count * IssuedCountRate * defaultIssuedRate), sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default issued rate (% of loans)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, defaultIssuedRate, sNumberFormat: TitledValue.Format.Percent);
			column = this.sheet.SetCellValue(row, column, defaultIssuedRate, sNumberFormat: TitledValue.Format.Percent);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default issued rate (% of approvals)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default outstanding", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, Amount * IssuedCountRate * defaultIssuedRate * OutstandingAmountRate, sNumberFormat: TitledValue.Format.Money);
			column = this.sheet.SetCellValue(row, column, Math.Round(Count * IssuedCountRate * defaultIssuedRate), sNumberFormat: TitledValue.Format.Int);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default outstanding rate (% of loans)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);

			row++;
			column = 1;

			column = this.sheet.SetCellValue(row, column, "Default outstanding rate (% of approvals)", true);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);
			column = this.sheet.SetCellValue(row, column, 0);

			for (int i = firstRow; i <= row; i++)
				for (int j = 1; j <= column; j++)
					SetBorder(this.sheet.Cells[i, j]);

			row++;

			return InsertDivider(row);
		} // DrawSummary
	} // class ManuallyRejectedAutoApproved
} // namespace
