namespace ExtractDataForLsa {
	using System;
	using System.IO;
	using Ezbob.Database;

	class SmsData : AResultRow {
		public string LoanID { get; set; }
		public DateTime DateSent { get; set; }
		public string Sid { get; set; }
		public string To { get; set; }
		public string Body { get; set; }

		public void SaveTo(string basePath) {
			string localBasePath = Path.Combine(basePath, LoanID, "sms");

			if (!Directory.Exists(localBasePath))
				Directory.CreateDirectory(localBasePath);

			string time = DateSent.ToString("yyyy-MM-dd_HH-mm-ss");

			string fileName = Path.Combine(localBasePath, time + "_" + Sid + ".txt");

			File.WriteAllText(fileName, "To: " + To + "\n" + Body, System.Text.Encoding.UTF8);
		} // SaveTo
	} // class SmsData
} // namespace
