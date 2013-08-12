using System;
using System.IO;
using Ezbob.HmrcHarvester;
using Ezbob.Logger;

namespace TestHarvester {
	class Program {
		static void Main(string[] args) {
			var oLog = new ConsoleLog(new LegacyLog());

			// TestDownload(oLog);

			TestParse(oLog);
		} // Main

		#region method TestParse

		private static void TestParse(ASafeLog oLog) {
			string sFilePath = Path.Combine(Directory.GetCurrentDirectory(), "files", "vatreturn-05_12.html");

			byte[] oFileData = File.ReadAllBytes(sFilePath);

			var vrt = new VatReturnThrasher(oLog);

			ISeeds parsed = vrt.Run(new SheafMetaData {
				DataType = DataType.VatReturn,
				FileType = FileType.Html,
				BaseFileName = "02 13",
				Thrasher = null
			}, oFileData);
		} // TestParse

		#endregion method TestParse

		#region method TestDownload

		private static void TestDownload(ASafeLog oLog) {
			var harvester = new Harvester("829144784260", "18june1974", oLog);

			if (harvester.Init()) {
				harvester.Run();

				string sBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "files");

				oLog.Info("{0} errors occured", harvester.Hopper.ErrorCount);

				foreach (DataType nDataType in Enum.GetValues(typeof (DataType))) {
					foreach (FileType nFileType in Enum.GetValues(typeof (FileType))) {
						harvester.Hopper.ForEachFile(nDataType, nFileType, (dt, ft, sBaseFileName, oData) => {
							string sFilePath = Path.Combine(sBaseDir, string.Format("{0}-{1}.{2}",
								dt.ToString().ToLower(), sBaseFileName, ft.ToString().ToLower()
							));

							oLog.Info("Saving {0}...", sFilePath);

							var sw = new BinaryWriter(File.Open(sFilePath, FileMode.Create));

							sw.Write(oData, 0, oData.Length);

							sw.Close();

							oLog.Info("Saving {0} complete.", sFilePath);
						});
					} // foreach file type
				} // for each data type
			} // if init

			harvester.Done();
		} // TestDownload

		#endregion method TestDownload
	} // class Program
} // namespace TestHarvester
