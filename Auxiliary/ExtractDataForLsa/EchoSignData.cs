namespace ExtractDataForLsa {
	using System;
	using System.IO;
	using Ezbob.Database;

	class EchoSignData : AResultRow {
		public string LoanID { get; set; }
		public DateTime SendDate { get; set; }
		public string StatusName { get; set; }
		public string FileNameBase { get; set; }
		public byte[] SignedDocument { get; set; }

		public void SaveTo(string basePath) {
			string localBasePath = Path.Combine(basePath, LoanID, "additional-agreements");

			if (!Directory.Exists(localBasePath))
				Directory.CreateDirectory(localBasePath);

			string time = SendDate.ToString("yyyy-MM-dd_HH-mm-ss");

			string fileName = Path.Combine(localBasePath, FileNameBase + "_" + time + ".pdf");

			File.WriteAllBytes(fileName, SignedDocument);
		} // SaveTo
	} // class EchoSignData
} // namespace
