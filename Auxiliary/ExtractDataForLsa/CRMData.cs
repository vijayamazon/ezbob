namespace ExtractDataForLsa {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using OfficeOpenXml;

	class CRMData : AResultRow {
		public string LoanID { get; set; }
		public int LoanInternalID { get; set; }
		public string Type { get; set; }
		public DateTime EventTime { get; set; }
		public string Action { get; set; }
		public string Status { get; set; }
		public string Rank { get; set; }
		public string PhoneNumber { get; set; }
		public string Comment { get; set; }

		public int SaveTo(ExcelWorksheet sheet, int row) {
			var lst = new List<object>();

			this.Traverse((instance, pi) => {
				lst.Add(pi.GetValue(instance));
			});

			sheet.SetRowValues(row, true, lst.ToArray());

			return row + 1;
		} // SaveTo

		public static ExcelWorksheet CreateSheet(ExcelPackage book) {
			var lst = new List<string>();

			PropertyTraverser.Traverse<CRMData>((ignored, pi) => {
				lst.Add(pi.Name);
			});

			return book.CreateSheet("CRM", false, lst.ToArray());
		} // CreateSheet
	} // class CRMData
} // namespace
