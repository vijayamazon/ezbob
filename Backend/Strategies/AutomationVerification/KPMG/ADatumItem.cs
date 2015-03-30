namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	public abstract class ADatumItem : SpLoadCashRequestsForAutomationReport.ResultRow {
		public abstract string DecisionStr { get; }

		public abstract bool IsAuto { get; }

		public static string CsvTitles(string prefix) {
			return string.Format(
				"{0} Cash Request ID;" +
				"{0} Time;{0} Medal;{0} Ezbob Score;{0} Decision;{0} Amount;{0} Interest Rate;" +
				"{0} Repayment Period;{0} Setup Fee %;{0} Setup Fee Amount",
				prefix
			);
		} // ToCsv

		public virtual int ToXlsx(ExcelWorksheet sheet, int rowNum, int colNum) {
			colNum = sheet.SetCellValue(rowNum, colNum, CashRequestID);
			colNum = sheet.SetCellValue(rowNum, colNum, DecisionTime);
			colNum = sheet.SetCellValue(rowNum, colNum, MedalName);
			colNum = sheet.SetCellValue(rowNum, colNum, EzbobScore);
			colNum = sheet.SetCellValue(rowNum, colNum, DecisionStr);
			colNum = sheet.SetCellValue(rowNum, colNum, ApprovedAmount);
			colNum = sheet.SetCellValue(rowNum, colNum, InterestRate);
			colNum = sheet.SetCellValue(rowNum, colNum, RepaymentPeriod);
			colNum = sheet.SetCellValue(rowNum, colNum, SetupFeePct);
			colNum = sheet.SetCellValue(rowNum, colNum, SetupFeeAmount);
			return colNum;
		} // ToCsv

		public virtual decimal SetupFeePct { get; protected set; }
		public virtual decimal SetupFeeAmount { get; protected set; }
	} // class MedalAndPricing
} // namespace
