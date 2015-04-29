namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.ExcelExt;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using OfficeOpenXml;

	internal class SheetRawAutomation {
		public SheetRawAutomation(ExcelPackage workbook, List<Datum> data) {
			this.sheet = workbook.CreateSheet(
				"Raw automation",
				false,
				"Cash request ID",
				"Manual decision",
				"Auto decision",
				"Auto re-reject",
				"Auto reject",
				"Auto re-approve",
				"Auto approve"
			);

			this.data = data;
			this.log = Library.Instance.Log;
		} // constructor

		public int Generate() {
			int row = 2;

			var pc = new ProgressCounter("{0} rows sent to Raw automation sheet.", this.log, 50);

			foreach (Datum d in this.data) {
				for (int i = 0; i < d.ItemCount; i++) {
					int column = 1;

					ManualDatumItem manual = d.Manual(i);
					AutoDatumItem auto = d.Auto(i);

					DecisionActions autoDecision = DecisionActions.Waiting;

					if (auto.IsAutoReRejected)
						autoDecision = DecisionActions.ReReject;
					else if (auto.IsAutoRejected)
						autoDecision = DecisionActions.Reject;
					else if (auto.IsAutoReApproved)
						autoDecision = DecisionActions.ReApprove;
					else if (auto.IsApproved)
						autoDecision = DecisionActions.Approve;

					column = this.sheet.SetCellValue(row, column, manual.CashRequestID);
					column = this.sheet.SetCellValue(row, column, manual.DecisionStr);
					column = this.sheet.SetCellValue(row, column, autoDecision.ToString());
					column = this.sheet.SetCellValue(row, column, auto.IsAutoReRejected ? DecisionActions.ReReject.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, auto.IsAutoRejected ? DecisionActions.Reject.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, auto.IsAutoReApproved ? DecisionActions.ReApprove.ToString() : "No");
					column = this.sheet.SetCellValue(row, column, auto.IsApproved ? DecisionActions.Approve.ToString() : "No");

					row++;

					pc++;
				} // for each item (i.e. cash request)
			} // for each datum (i.e. aggregated decision)

			pc.Log();

			return row;
		} // Generate

		private readonly ExcelWorksheet sheet;
		private readonly List<Datum> data;
		private readonly ASafeLog log;
	} // class SheetRawAutomation
} // namespace
