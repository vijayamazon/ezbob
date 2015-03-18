namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using Ezbob.ExcelExt;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using OfficeOpenXml;

	internal class LoanSummaryData : LoanMetaData {
		public LoanSummaryData() {
			Counter = 0;
		} // constructor

		public void Add(LoanMetaData lmd) {
			if (Counter == 0) {
				LoanAmount = lmd.LoanAmount;
				RepaidPrincipal = lmd.RepaidPrincipal;
				LoanStatus = lmd.LoanStatus;
				MaxLateDays = lmd.MaxLateDays;
			} else {
				LoanAmount += lmd.LoanAmount;
				RepaidPrincipal += lmd.RepaidPrincipal;
				MaxLateDays = Math.Max(MaxLateDays, lmd.MaxLateDays);

				if (lmd.LoanStatus == LoanStatus.Late)
					LoanStatus = LoanStatus.Late;
				else if (lmd.LoanStatus == LoanStatus.Live) {
					if (LoanStatus != LoanStatus.Late)
						LoanStatus = LoanStatus.Live;
				} else {
					if ((LoanStatus != LoanStatus.Late) && (LoanStatus != LoanStatus.Live))
						LoanStatus = LoanStatus.PaidOff;
				} // if
			} // if

			Counter++;
		} // Add

		public int Counter { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Join(";",
				Counter,
				LoanStatus.ToString(),
				LoanAmount,
				RepaidPrincipal,
				MaxLateDays
			);
		} // ToString

		public int ToXlsx(ExcelWorksheet sheet, int rowNum, int curColumn) {
			curColumn = sheet.SetCellValue(rowNum, curColumn, Counter);
			curColumn = sheet.SetCellValue(rowNum, curColumn, LoanStatus.ToString());
			curColumn = sheet.SetCellValue(rowNum, curColumn, LoanAmount);
			curColumn = sheet.SetCellValue(rowNum, curColumn, RepaidPrincipal);
			curColumn = sheet.SetCellValue(rowNum, curColumn, MaxLateDays);
			return curColumn;
		} // ToString
	} // LoanSummaryData
} // namespace
