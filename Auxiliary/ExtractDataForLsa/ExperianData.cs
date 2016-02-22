namespace ExtractDataForLsa {
	using System;
	using System.IO;
	using Ezbob.Database;

	class ExperianData : AResultRow {
		public string LoanID { get; set; }
		public string ServiceType { get; set; }
		public DateTime FetchTime { get; set; }
		public string ExperianRawData { get; set; }

		public void SaveTo(string basePath) {
			string type = ServiceType.Replace(" ", "-");

			string localBasePath = Path.Combine(basePath, LoanID, "Experian", type);

			if (!Directory.Exists(localBasePath))
				Directory.CreateDirectory(localBasePath);

			string time = FetchTime.ToString("yyyy-MM-dd_HH-mm-ss");

			string fileName = Path.Combine(localBasePath, time + ".xml");

			File.WriteAllText(fileName, ExperianRawData, System.Text.Encoding.UTF8);
		} // SaveTo
	} // class ExperianData
} // namespace
