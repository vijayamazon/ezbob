namespace ExtractDataForLsa {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using OfficeOpenXml;

	class DirectorData : AResultRow {
		public string LoanID { get; set; }
		public int LoanInternalID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public bool IsDirector { get; set; }
		public bool IsShareholder { get; set; }
		public string Address_Line1 { get; set; }
		public string Address_Line2 { get; set; }
		public string Address_Line3 { get; set; }
		public string Address_Town { get; set; }
		public string Address_County { get; set; }
		public string Address_Postcode { get; set; }
		public string Address_Country { get; set; }

		public string FullName {
			get {
				var lst = new List<string> {
					FirstName,
					MiddleName,
					LastName,
				};

				return string.Join(" ", lst.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
			} // get
		} // FullName

		public int SaveTo(ExcelWorksheet sheet, int column) {
			int row = 2;

			this.Traverse((instance, pi) => {
				sheet.SetCellValue(row, 1, pi.Name);
				sheet.SetCellValue(row, column, pi.GetValue(instance));

				row++;
			});

			return column + 1;
		} // SaveTo
	} // class CustomerData
} // namespace
