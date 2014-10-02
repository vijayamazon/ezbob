namespace FullCustomerModelGreps {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Text.RegularExpressions;
	using Ezbob.Logger;

	class Program {
		static void Main(string[] args) {
			ms_bExtractingData = false;
			ASafeLog oLog = new ConsoleLog(new FileLog("FullCustomerModelGreps"));
			oLog.NotifyStart();

			if (args.Length < 1)
				oLog.Fatal("No path specified.");
			else {
				var oData = new SortedDictionary<string, Stat>();
				ProcessDir(oData, args[0], oLog);

				oLog.Info("Step: Count;Min;Average;Median;Pct75;Pct90;Max");

				foreach (var pair in oData) {
					pair.Value.SetAverage();

					oLog.Info("{0}: {1}", pair.Key, pair.Value);
				} // for
			} // if

			oLog.NotifyStop();
		} // Main

		private static void ProcessDir(SortedDictionary<string, Stat> oStat, string sPath, ASafeLog oLog) {
			oLog.Info("Processing directory {0} started...", sPath);

			if (!Directory.Exists(sPath))
				oLog.Alert("Directory not found: {0}.", sPath);
			else {
				string[] aryFileNames = Directory.GetFiles(sPath, "EzBob.Web.log.201*.zip");

				FileNameComparer fnc = new FileNameComparer();

				Array.Sort<string>(aryFileNames, fnc);

				foreach (var sFileName in aryFileNames)
					ReadFromZip(oStat, sFileName, oLog);

				string[] aryDirNames = Directory.GetDirectories(sPath);

				foreach (string sDirName in aryDirNames) {
					oLog.Info("Subdirectory found: {0}.", sDirName);

					ProcessDir(oStat, sDirName, oLog);
				} // for each
			} // if

			oLog.Info("Processing directory {0} complete.", sPath);
		} // ProcessDir

		private static void ReadFromZip(SortedDictionary<string, Stat> oStat, string sFileName, ASafeLog oLog) {
			oLog.Msg("Processing zip file {0}...", sFileName);

			using (FileStream zipToOpen = new FileStream(sFileName, FileMode.Open))
				using (ZipArchive zip = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
					foreach (ZipArchiveEntry entry in zip.Entries)
						ProcessFile(oStat, entry, oLog);

			oLog.Msg("Processing zip file {0} complete.", sFileName);
		} // ReadFromZip

		private static void ProcessFile(SortedDictionary<string, Stat> oStat, ZipArchiveEntry oEntry, ASafeLog oLog) {
			StreamReader sr = new StreamReader(oEntry.Open());

			for (;;) {
				string sLine = sr.ReadLine();

				if (sLine == null)
					break;

				if (ms_bExtractingData)
					ms_bExtractingData = ExtractData(oStat, sLine, oLog);
				else if (sLine.Contains("FullCustomerModel building time for customer")) {
					ms_CurrentCustomer = sLine;
					ms_bExtractingData = true;
				} // if
			} // for

			sr.Close();
		} // ProcessFile

		private static bool ms_bExtractingData;
		private static string ms_CurrentCustomer;

		private static bool ExtractData(SortedDictionary<string, Stat> oStat, string sLine, ASafeLog oLog) {
			bool bStillExtracting = true;

			string[] ary = sLine.Split(':');

			// oLog.Debug("{0}", sLine); if (ary.Length != 2) oLog.Fatal("{1}: two fields expected in {0}", sLine, ms_CurrentCustomer);

			string sCaption = ary[0].Trim();
			double nLength = Convert.ToDouble(ary[1].Replace("ms", ""));

			if (sCaption.StartsWith("Customer ")) {
				sCaption = "Total";
				bStillExtracting = false;
			} // if

			if (oStat.ContainsKey(sCaption))
				oStat[sCaption].Append(nLength);
			else
				oStat[sCaption] = new Stat(nLength);

			return bStillExtracting;
		} // ExtractData

		private class FileNameComparer : IComparer<string> {
			public int Compare(string x, string y) {
				FileDate a = new FileDate(x);
				FileDate b = new FileDate(y);
				return a.CompareTo(b);
			} // Compare
		} // class FileNameComparer

		private class FileDate {
			public FileDate(string sFileName) {
				Match m = Re.Match(sFileName);

				Year = Convert.ToInt32(m.Groups[1].Value);
				Month = Convert.ToInt32(m.Groups[2].Value);
				Day = Convert.ToInt32(m.Groups[3].Value);

				if (!string.IsNullOrWhiteSpace(m.Groups[5].Value))
					FileNo = Convert.ToInt32(m.Groups[5].Value);
			} // constructor

			public override string ToString() {
				return string.Format("{0}-{1}-{2}.{3}", Year, Month, Day, FileNo);
			} // ToString

			public int CompareTo(FileDate a) {
				if (Year != a.Year)
					return Year.CompareTo(a.Year);

				if (Month != a.Month)
					return Month.CompareTo(a.Month);

				if (Day != a.Day)
					return Day.CompareTo(a.Day);

				return a.FileNo.CompareTo(FileNo);
			} // CompareTo

			private int Year { get; set; }
			private int Month { get; set; }
			private int Day { get; set; }
			private int FileNo { get; set; }

			private static readonly Regex Re = new Regex(@"EzBob\.Web\.log\.(201\d)-(\d\d)-(\d\d)(.(\d+))?\.zip$");
		} // FileDate
	} // class Program
} // namespace
