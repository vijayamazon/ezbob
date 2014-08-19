namespace VerifyUploadedFiles {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using ConfigManager;
	using Ezbob.Logger;

	class Program {
		static void Main(string[] args) {
			ASafeLog oLog = new ConsoleLog(new FileLog("VerifyUploadedFiles"));
			var oErrors = new SortedDictionary<string, string>();
			var oPassed = new SortedDictionary<string, string>();

			var oLimits = new OneUploadLimitation(10000000, "image/*,application/pdf,.doc,.docx,.odt,.rtf,.ppt,.pptx,.odp,.xls,.xlsx,.ods,.txt,.csv,.xml,.htm,.html,.xhtml,.mht,.msg,.eml");

			oLog.Info("File limitations are: {0}", oLimits);

			ProcessDir(oLimits, args.Length > 0 ? args[0] : Directory.GetCurrentDirectory(), oPassed, oErrors, oLog);

			oLog.Info("Passed files - begin:");

			foreach (KeyValuePair<string, string> pair in oPassed)
				oLog.Info("\t{0}: {1}", pair.Key, pair.Value);

			oLog.Info("Passed files - end.");

			oLog.Info("Failed files - begin:");

			foreach (KeyValuePair<string, string> pair in oErrors)
				oLog.Info("\t{0}: {1}", pair.Key, pair.Value);

			oLog.Info("Failed files - end.");
		} // Main

		private static bool IsDir(string sPath) {
			return (File.GetAttributes(sPath) & FileAttributes.Directory) == FileAttributes.Directory;
		} // IsDir

		private static void ProcessDir(OneUploadLimitation oLimits, string sPath, SortedDictionary<string, string> oPassed, SortedDictionary<string, string> oErrors, ASafeLog oLog) {
			IEnumerable<string> lst;

			try {
				lst = Directory.EnumerateFileSystemEntries(sPath);
			}
			catch (Exception e) {
				oLog.Alert(e, "Failed to enumerate objects in {0}.", sPath);
				return;
			} // try

			foreach (string sFileName in lst) {
				if (IsDir(sFileName))
					ProcessDir(oLimits, sFileName, oPassed, oErrors, oLog);
				else
					ProcessFile(oLimits, sFileName, oPassed, oErrors, oLog);
			} // for each
		} // ProcessFile

		private static void ProcessFile(
			OneUploadLimitation oLimits,
			string sFileName,
			SortedDictionary<string, string> oPassed,
			SortedDictionary<string, string> oErrors,
			ASafeLog oLog
		) {
			try {
				oLog.Info("Examining file {0}...", sFileName);

				var fi = new FileInfo(sFileName);

				var os = new List<string>();

				if (fi.Length > oLimits.FileSize)
					os.Add("file size " + fi.Length.ToString("N0", CultureInfo.InvariantCulture));

				var oBuf = new byte[256];

				FileStream ins = File.OpenRead(sFileName);
				var nRead = ins.Read(oBuf, 0, oBuf.Length);
				ins.Close();

				string sMimeType = oLimits.DetectFileMimeType(oBuf, sFileName, nRead, oLog);

				if (string.IsNullOrWhiteSpace(sMimeType))
					os.Add("MIME type");

				string sMsg;

				if (os.Count > 0) {
					sMsg = string.Join(" and ", os);
					oErrors[sFileName] = sMsg;
				}
				else {
					sMsg = "size " + fi.Length.ToString("N0", CultureInfo.InvariantCulture) + " bytes of type " + sMimeType;
					oPassed[sFileName] = sMsg;
				}

				oLog.Info("File {0} has {1}.", sFileName, os.Count == 0 ? "passed with " + sMsg : "failed due to " + sMsg);
			}
			catch (Exception e) {
				oLog.Alert(e, "Failed to process file {0}", sFileName);
			} // try
		} // ProcessFile
	} // class Program
} // namespace
