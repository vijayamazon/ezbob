namespace Reports.Alibaba.Funnel {
	using System.Collections.Generic;
	using OfficeOpenXml;

	internal class StrInt {
		public virtual string Caption { get; set; }
		public virtual int Counter { get; set; }

		public virtual void SaveTo(ExcelWorksheet oSheet) {
			int nRow = 2;

			while (oSheet.Cells[nRow, 1].Value != null)
				nRow++;

			int nColumn = oSheet.SetRowValues(nRow, true, 
				Caption,
				Counter
			);

			foreach (object obj in GetAdditionalReportFields()) {
				if (obj is double)
					oSheet.Cells[nRow, nColumn].Style.Numberformat.Format = "0.0%";

				nColumn = oSheet.SetCellValue(nRow, nColumn, obj);
			} // for each
		} // SaveTo

		public virtual IEnumerable<object> GetAdditionalReportFields() {
			return new List<object>();
		} // GetAdditionalReportFields
	} // class StrInt
} // namespace
