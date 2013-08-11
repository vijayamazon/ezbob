using System;
using System.IO;
using Ezbob.HmrcHarvester;
using Ezbob.Logger;

namespace TestHarvester {
	class Program {
		static void Main(string[] args) {
			var oLog = new ConsoleLog(new LegacyLog());
			var harvester = new Harvester("829144784260", "18june1974", oLog);

			if (harvester.Init()) {
				harvester.Run();

				string sBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "files");

				oLog.Info("{0} errors occured", harvester.Hopper.ErrorCount);

				foreach (Hopper.DataType nDataType in Enum.GetValues(typeof (Hopper.DataType))) {
					foreach (Hopper.FileType nFileType in Enum.GetValues(typeof (Hopper.FileType))) {
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
		} // Main
	} // class Program
} // namespace TestHarvester
