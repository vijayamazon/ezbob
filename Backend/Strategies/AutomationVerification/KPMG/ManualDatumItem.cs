namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using OfficeOpenXml;

	public class ManualDatumItem : ADatumItem {
		public ManualDatumItem(
			SpLoadCashRequestsForAutomationReport.ResultRow sr,
			string tag,
			ASafeLog log
		) : base(tag, log.Safe()) {
			sr.CopyTo(this);
		} // constructor

		public override string DecisionStr {
			get { return IsApproved ? "Approved" : "Rejected"; }
		} // DecisionStr

		public override bool IsAuto { get { return false; } }

		public virtual void Calculate() {
			SetupFeeAmount = new SetupFeeCalculatorLegacy(
				UseSetupFee == 1,
				UseBrokerSetupFee,
				ManualSetupFeeAmount,
				ManualSetupFeePercent
			).Calculate(ApprovedAmount, true);

			SetupFeePct = ApprovedAmount <= 0 ? 0 : SetupFeeAmount / ApprovedAmount;
		} // Calculate

		public static new string CsvTitles(string prefix) {
			return string.Format("{0};{1} manual loan count", ADatumItem.CsvTitles(prefix + " manual"), prefix);
		} // CsvTitles

		public override int ToXlsx(ExcelWorksheet sheet, int rowNum, int colNum) {
			colNum = base.ToXlsx(sheet, rowNum, colNum);
			colNum = sheet.SetCellValue(rowNum, colNum, CrLoanCount);
			return colNum;
		} // ToXlsx
	} // ManualDatumItem
} // namespace
