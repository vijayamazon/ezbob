namespace EzBob.Web.Infrastructure.Hmrc {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Web;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;

	internal class HmrcFileProcessor {
		public HmrcFileProcessor(
			int nCustomerID,
			HttpFileCollectionBase oFiles,
			string sControllerName,
			string sActionName
		) {
			CustomerID = nCustomerID;
			FileList = oFiles;
			FileCache = new HmrcFileCache();
			ErrorMsg = null;
			m_oLimitations = CurrentValues.Instance.GetUploadLimitations(sControllerName, sActionName);
		} // constructor

		public void Run() {
			FileCache.ErrorMsg = string.Empty;

			for (int i = 0; i < FileList.Count; i++) {
				HttpPostedFileBase oFile = FileList[i];

				if (oFile == null) {
					ms_oLog.Debug("File {0}: not found, ignoring.", i);
					continue;
				} // if

				ms_oLog.Debug("File {0}, name: {1}", i, oFile.FileName);

				if (oFile.ContentLength == 0) {
					ms_oLog.Debug("File {0}: is empty, ignoring.", i);
					continue;
				} // if

				var oFileContents = new byte[oFile.ContentLength];

				int nRead = oFile.InputStream.Read(oFileContents, 0, oFile.ContentLength);

				if (nRead != oFile.ContentLength) {
					ms_oLog.Warn("File {0}: failed to read entire file contents, ignoring.", i);
					continue;
				} // if

				string sMimeType = m_oLimitations.DetectFileMimeType(oFileContents, oFile.FileName, oLog: ms_oLog);

				ms_oLog.Debug("File {0}, name: {1}, MIME type {2}", i, oFile.FileName, sMimeType);

				if (string.IsNullOrWhiteSpace(sMimeType)) {
					ms_oLog.Debug("File {0}: has unsupported content type, ignoring.", i);
					continue;
				} // if
				
				SaveToDisc(CustomerID, OneUploadLimitation.FixFileName(oFile.FileName), oFileContents);

				var smd = new SheafMetaData {
					BaseFileName = OneUploadLimitation.FixFileName(oFile.FileName),
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null
				};

				FileCache.Add(smd, oFileContents);

				var vrpt = new VatReturnPdfThrasher(false, ms_oLog);
				ISeeds oResult;

				try {
					oResult = vrpt.Run(smd, oFileContents);
				}
				catch (Exception e) {
					ms_oLog.Warn(e, "Failed to parse file {0} named {1}:", i, oFile.FileName);
					continue;
				} // try

				if (oResult == null) {
					ErrorMsg = m_sErrorMsg + " " + ((VatReturnSeeds)vrpt.Seeds).FatalError;
					continue;
				} // if

				var oSeeds = (VatReturnSeeds)oResult;

				var di = new DateInterval(oSeeds.DateFrom, oSeeds.DateTo);

				ms_oLog.Debug("HMRC file cache state before adding file {0}: {1}.", oFile.FileName, FileCache);

				if (FileCache.Intersects(oSeeds.RegistrationNo, di))
					return;

				FileCache.Add(oSeeds.RegistrationNo, di, smd, oResult);

				ms_oLog.Debug("HMRC file cache state after adding file {0}: {1}.", oFile.FileName, FileCache);
			} // for
		} // Run

		public string ErrorMsg {
			get { return FileCache.ErrorMsg; }
			private set { m_sErrorMsg = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
		} // ErroMsg

		private string m_sErrorMsg;

		public Hopper Hopper {
			get { return FileCache.Hopper; }
		} // Hopper

		public int AddedCount {
			get { return FileCache.AddedCount; }
		} // AddedCount

		private HttpFileCollectionBase FileList { get; set; } // FileList
		private int CustomerID { get; set; } // CustomerID
		private HmrcFileCache FileCache { get; set; } // FileCache

		private readonly OneUploadLimitation m_oLimitations;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(HmrcFileProcessor));

		private static void SaveToDisc(int nCustomerID, string sFileOriginalName, byte[] oFileContents) {
			try {
				ms_oLog.Debug("Saving file {0} to disc...", sFileOriginalName);

				string sPath = CurrentValues.Instance.HmrcUploadedFilesSavePath;

				if (string.IsNullOrWhiteSpace(sPath))
					ms_oLog.Debug("Not saving: operation is disabled (HmrcUploadedFilesSavePath is empty).");
				else {
					try {
						Directory.CreateDirectory(sPath);
					}
					catch (Exception e) {
						ms_oLog.Warn(e, "Error while creating directory.");
					} // try

					if (Directory.Exists(sPath)) {
						string sFileName = Path.Combine(
							sPath,
							Guid.NewGuid().ToString("N") + "." + nCustomerID + "." + sFileOriginalName
						);

						ms_oLog.Debug("Saving file {0} as {1}...", sFileOriginalName, sFileName);

						File.WriteAllBytes(sFileName, oFileContents);
					}
					else
						ms_oLog.Error("Cannot save file " + sFileOriginalName + ": target directory does not exist.");
				} // if

				ms_oLog.Debug("Saving file {0} to disc complete.", sFileOriginalName);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Error saving file '" + sFileOriginalName + "' to disc.");
			} // try
		} // SaveToDisc

		private class HmrcFileCache {

			public HmrcFileCache() {
				ErrorMsg = string.Empty;
				Hopper = new Hopper(VatReturnSourceType.Uploaded);
				m_oDateIntervals = new SortedDictionary<long, List<DateInterval>>();
				AddedCount = 0;
			} // HmrcFileCache

			public string ErrorMsg {
				get { return m_sErrorMsg; } // get
				set { m_sErrorMsg = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
			} // ErroMsg

			private string m_sErrorMsg;

			public Hopper Hopper { get; private set; } // Hopper

			public int AddedCount { get; private set; } // AddedCount

			public void Add(SheafMetaData smd, byte[] oFileContents) {
				Hopper.Add(smd, oFileContents);
			} // Add

			public void Add(long nRegistrationNo, DateInterval di, SheafMetaData smd, ISeeds oSeeds) {
				Hopper.Add(smd, oSeeds);

				if (m_oDateIntervals.ContainsKey(nRegistrationNo))
					m_oDateIntervals[nRegistrationNo].Add(di);
				else
					m_oDateIntervals[nRegistrationNo] = new List<DateInterval> { di };

				AddedCount++;
			} // Add

			public bool Intersects(long nRegistrationNo, DateInterval di) {
				if (!m_oDateIntervals.ContainsKey(nRegistrationNo))
					return false;

				foreach (DateInterval oInterval in m_oDateIntervals[nRegistrationNo]) {
					if (oInterval.Intersects(di)) {
						Hopper.Clear();
						m_oDateIntervals.Clear();
						ErrorMsg = "Inconsistent date ranges: " + oInterval + " and " + di + ".";
						ms_oLog.Warn("HMRC file cache has been cleaned because of error: {0}", ErrorMsg);
						return true;
					} // if
				} // for each

				return false;
			} // Intersects

			public override string ToString() {
				var os = new StringBuilder();

				os.AppendFormat("cache contains {0} file{1}", AddedCount, AddedCount == 1 ? "" : "s");

				if (AddedCount > 0) {
					os.Append("; date intervals are:");

					foreach (var pair in m_oDateIntervals)
						os.AppendFormat(" ({0}: {1})", pair.Key, string.Join(", ", pair.Value));
				} // if

				if (!string.IsNullOrWhiteSpace(ErrorMsg))
					os.AppendFormat("; error message: {0}", ErrorMsg);

				return os.ToString();
			} // ToString

			private readonly SortedDictionary<long, List<DateInterval>> m_oDateIntervals;
		} // HmrcFileCache
	} // class HmrcFileProcessor
} // namespace EzBob.Web.Areas.Customer.Controllers
