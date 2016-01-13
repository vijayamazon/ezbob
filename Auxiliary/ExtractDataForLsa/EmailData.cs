namespace ExtractDataForLsa {
	using System;
	using System.IO;
	using Ezbob.Database;

	class EmailData : AResultRow {
		public string LoanID { get; set; }
		public long EmailID { get; set; }
		public string FileName { get; set; }
		public Byte[] BinaryBody { get; set; }
		public DateTime CreationDate { get; set; }

		public void SaveTo(string basePath) {
			string localBasePath = Path.Combine(basePath, LoanID, "emails");

			if (!Directory.Exists(localBasePath))
				Directory.CreateDirectory(localBasePath);

			string time = CreationDate.ToString("yyyy-MM-dd_HH-mm-ss");

			string fileName = Path.Combine(localBasePath, EmailID + "_" + time + "_" + FileName);

			File.WriteAllBytes(fileName, BinaryBody);
		} // SaveTo
	} // class EmailData
} // namespace
