namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	public abstract class ADatumItem : SpLoadCashRequestsForAutomationReport.ResultRow {
		public abstract string DecisionStr { get; }

		public abstract bool IsAuto { get; }

		public static string CsvTitles(string prefix) {
			return string.Format(
				"{0} cash request ID;" +
				"{0} time;{0} medal;{0} Ezbob score;{0} decision;{0} amount;{0} interest rate;" +
				"{0} repayment period;{0} setup fee %;{0} setup fee amount",
				prefix
			);
		} // CsvTitles

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
		} // ToXlsx

		public virtual decimal SetupFeePct { get; protected set; }
		public virtual decimal SetupFeeAmount { get; protected set; }

		protected ADatumItem(string tag, ASafeLog log) {
			Tag = tag;
			Log = log.Safe();
		} // constructor

		protected virtual string Tag { get; private set; }

		protected virtual ASafeLog Log { get; private set; }
	} // class MedalAndPricing
} // namespace
