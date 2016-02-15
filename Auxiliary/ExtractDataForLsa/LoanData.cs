namespace ExtractDataForLsa {
	using System.Collections.Generic;
	using System.IO;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;

	class LoanData {
		public LoanData(string loanID, int internalID) {
			LoanID = loanID;
			LoanInternalID = internalID;
			Directors = new List<DirectorData>();
			Crm = new List<CRMData>();
		} // constructor

		public string LoanID { get; private set; }
		public int LoanInternalID { get; private set; }

		public CustomerData CustomerData { get; set; }

		public List<DirectorData> Directors { get; private set; }

		public List<CRMData> Crm { get; private set; }

		public void SaveTo(string basePath) {
			string fullDirName = Path.Combine(basePath, LoanID);

			if (!Directory.Exists(fullDirName))
				Directory.CreateDirectory(fullDirName);

			ExcelPackage book = new ExcelPackage();

			ExcelWorksheet sheet = book.CreateSheet("Provided data", false, "Name", "Value");
			CustomerData.SaveTo(sheet);

			if (Directors.Count > 0) {
				var titles = new List<string> { "Name", };

				for (int i = 1; i <= Directors.Count; i++)
					titles.Add(string.Format("Director #{0}", i));

				sheet = book.CreateSheet("Directors", false, titles.ToArray());

				int column = 2;

				foreach (DirectorData dir in Directors)
					column = dir.SaveTo(sheet, column);
			} // if

			if (Crm.Count > 0) {
				sheet = CRMData.CreateSheet(book);

				int row = 2;

				foreach (CRMData crm in Crm)
					row = crm.SaveTo(sheet, row);
			} // if

			book.AutoFitColumns();

			book.SaveAs(new FileInfo(Path.Combine(fullDirName, LoanID + ".xlsx")));
		} // SaveTo
	} // class LoanData
} // namespace
