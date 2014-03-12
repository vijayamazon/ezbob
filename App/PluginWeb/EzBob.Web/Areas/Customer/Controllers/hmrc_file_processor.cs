namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Web;
	using EZBob.DatabaseLib;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;
	using log4net;

	#region class HmrcFileCache

	internal class HmrcFileCache {
		#region public

		#region method Get

		public static HmrcFileCache Get(HttpSessionStateBase oSession) {
			return oSession[HmrcFileCache.Name] as HmrcFileCache;
		} // Get

		#endregion method Get

		#region method Clean

		public static void Clean(HttpSessionStateBase oSession)
		{
			oSession[HmrcFileCache.Name] = new HmrcFileCache();
		} // Get

		#endregion method Clean

		#region constructor

		public HmrcFileCache(HttpSessionStateBase oSession = null) {
			ErrorMsg = string.Empty;
			Hopper = new Hopper();
			DateIntervals = new List<DateInterval>();

			if (oSession != null)
				oSession[HmrcFileCache.Name] = this;
		} // HmrcFileCache

		#endregion constructor

		#region method SetError

		public void SetError(string sErrorMsg) {
			Hopper.Clear();
			DateIntervals.Clear();
			ErrorMsg = sErrorMsg;

			if (!string.IsNullOrWhiteSpace(ErrorMsg))
				ms_oLog.WarnFormat("HMRC file cache has been cleaned because of error: {0}", ErrorMsg);
		} // ClearData

		#endregion method SetError

		#region property ErrorMsg

		public string ErrorMsg {
			get { return m_sErrorMsg; } // get
			set { m_sErrorMsg = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
		} // ErroMsg

		private string m_sErrorMsg;

		#endregion property ErrorMsg

		#region property Hopper

		public Hopper Hopper { get; private set; } // Hopper

		#endregion property Hopper

		#region property DateIntervals

		public List<DateInterval> DateIntervals { get; private set; } // DateIntervals

		#endregion property Hopper

		#region property AddedCount

		public int AddedCount {
			get { return DateIntervals.Count; } // get
		} // AddedCount

		#endregion property AddedCount

		#region method Add

		public void Add(SheafMetaData smd, byte[] oFileContents) {
			Hopper.Add(smd, oFileContents);
		} // Add

		public void Add(DateInterval di, SheafMetaData smd, ISeeds oSeeds) {
			Hopper.Add(smd, oSeeds);
			DateIntervals.Add(di);
		} // Add

		#endregion method Add

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("cache contains {0} file{1}", AddedCount, AddedCount == 1 ? "" : "s");

			if (AddedCount > 0)
				os.AppendFormat("; date intervals are: {0}", string.Join(", ", DateIntervals));

			if (!string.IsNullOrWhiteSpace(ErrorMsg))
				os.AppendFormat("; error msg: {0}", ErrorMsg);

			return os.ToString();
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof (HmrcFileCache));

		private const string Name = "HmrcFileCache";

		#endregion private
	} // HmrcFileCache

	#endregion class HmrcFileCache

	#region class AHmrcFileProcessor

	internal abstract class AHmrcFileProcessor {
		#region public

		#region opertor cast to HmrcController.ValidateFilesResult

		public static implicit operator HmrcController.ValidateFilesResult(AHmrcFileProcessor hfp) {
			return new HmrcController.ValidateFilesResult {
				Hopper = hfp.FileCache.Hopper,
				Error = hfp.FileCache.ErrorMsg,
			};
		} // operator cast to HmrcController.ValidateFilesResult

		#endregion opertor cast to HmrcController.ValidateFilesResult

		public abstract HmrcFileCache FileCache { get; } // FileCache

		#region method Run

		public virtual void Run() {
			FileCache.ErrorMsg = string.Empty;

			long? nRegistrationNo = null;

			for (int i = 0; i < FileList.Count; i++) {
				HttpPostedFileBase oFile = FileList[i];

				if (oFile == null) {
					Log.DebugFormat("File {0}: not found, ignoring.", i);
					continue;
				} // if

				Log.DebugFormat("File {0}, name: {1}", i, oFile.FileName);

				if (oFile.ContentLength == 0) {
					Log.DebugFormat("File {0}: is empty, ignoring.", i);
					continue;
				} // if

				if (oFile.ContentType.Trim().ToLower() != "application/pdf") {
					Log.DebugFormat("File {0}: is not PDF content type, ignoring.", i);
					continue;
				} // if

				var oFileContents = new byte[oFile.ContentLength];

				int nRead = oFile.InputStream.Read(oFileContents, 0, oFile.ContentLength);

				if (nRead != oFile.ContentLength) {
					Log.WarnFormat("File {0}: failed to read entire file contents, ignoring.", i);
					continue;
				} // if

				SaveToDisc(CustomerID, oFile.FileName, oFileContents);

				var smd = new SheafMetaData {
					BaseFileName = oFile.FileName,
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null
				};

				FileCache.Add(smd, oFileContents);

				var vrpt = new VatReturnPdfThrasher(false, new SafeILog(Log));
				ISeeds oResult;

				try {
					oResult = vrpt.Run(smd, oFileContents);
				}
				catch (Exception e) {
					Log.WarnFormat("Failed to parse file {0} named {1}:", i, oFile.FileName);
					Log.Warn(e);
					continue;
				} // try

				if (oResult == null)
					continue;

				var oSeeds = (VatReturnSeeds)oResult;

				if (nRegistrationNo.HasValue) {
					if (nRegistrationNo.Value != oSeeds.RegistrationNo) {
						FileCache.SetError("Inconsistent business registration number.");
						return;
					} // if
				}
				else
					nRegistrationNo = oSeeds.RegistrationNo;

				var di = new DateInterval(oSeeds.DateFrom, oSeeds.DateTo);

				Log.DebugFormat("HMRC file cache state before adding file {0}: {1}.", oFile.FileName, FileCache);

				foreach (DateInterval oInterval in FileCache.DateIntervals) {
					if (oInterval.Intersects(di)) {
						FileCache.SetError("Inconsistent date ranges: " + oInterval + " and " + di);
						return;
					} // if
				} // for each

				FileCache.Add(di, smd, oResult);

				Log.DebugFormat("HMRC file cache state after adding file {0}: {1}.", oFile.FileName, FileCache);
			} // for
		} // Run

		#endregion method Run

		#endregion public

		#region protected

		#region constructor

		protected AHmrcFileProcessor(int nCustomerID, HttpFileCollectionBase oFiles, HttpSessionStateBase oSession = null) {
			CustomerID = nCustomerID;
			FileList = oFiles;
			Session = oSession;
		} // constructor

		#endregion constructor

		#region property Session

		protected virtual HttpSessionStateBase Session { get; private set; } // Session

		#endregion property Session

		protected static readonly ILog Log = LogManager.GetLogger(typeof(AHmrcFileProcessor));

		#endregion protected

		#region private

		private HttpFileCollectionBase FileList { get; set; } // FileList
		private int CustomerID { get; set; } // CustomerID

		#region method SaveToDisc

		private static void SaveToDisc(int nCustomerID, string sFileOriginalName, byte[] oFileContents) {
			try {
				Log.DebugFormat("Saving file {0} to disc...", sFileOriginalName);

				string sPath = DBConfigurationValues.Instance.HmrcUploadedFilesSavePath;

				if (string.IsNullOrWhiteSpace(sPath))
					Log.Debug("Not saving: operation is disabled (HmrcUploadedFilesSavePath is empty).");
				else {
					try {
						Directory.CreateDirectory(sPath);
					}
					catch (Exception e) {
						Log.Warn("Error while creating directory: ", e);
					} // try

					if (Directory.Exists(sPath)) {
						string sFileName = Path.Combine(sPath, Guid.NewGuid().ToString("N") + "." + nCustomerID + "." + sFileOriginalName);

						Log.DebugFormat("Saving file {0} as {1}...", sFileOriginalName, sFileName);

						File.WriteAllBytes(sFileName, oFileContents);
					}
					else
						Log.Error("Cannot save file " + sFileOriginalName + ": target directory does not exist.");
				} // if

				Log.DebugFormat("Saving file {0} to disc complete.", sFileOriginalName);
			}
			catch (Exception e) {
				Log.Error("Error saving file '" + sFileOriginalName + "' to disc: ", e);
			} // try
		} // SaveToDisc

		#endregion method SaveToDisc

		#endregion private
	} // class AHmrcFileProcessor

	#endregion class AHmrcFileProcessor

	#region class SessionHmrcFileProcessor

	internal class SessionHmrcFileProcessor : AHmrcFileProcessor {
		#region constructor

		public SessionHmrcFileProcessor(HttpSessionStateBase oSession, int nCustomerID, HttpFileCollectionBase oFiles) : base(nCustomerID, oFiles, oSession) {
		} // constructor

		#endregion constructor

		#region property FileCache

		public override HmrcFileCache FileCache {
			get {
				HmrcFileCache oFileCache = HmrcFileCache.Get(Session);

				if (oFileCache == null)
					oFileCache = new HmrcFileCache(Session);

				return oFileCache;
			} // get
		} // HmrcFileCache

		#endregion property FileCache
	} // SessionHmrcFileProcessor

	#endregion class SessionHmrcFileProcessor

	#region class LocalHmrcFileProcessor

	internal class LocalHmrcFileProcessor : AHmrcFileProcessor {
		#region constructor

		public LocalHmrcFileProcessor(int nCustomerID, HttpFileCollectionBase oFiles) : base(nCustomerID, oFiles) {
		} // constructor

		#endregion constructor

		#region property FileCache

		public override HmrcFileCache FileCache {
			get {
				if (m_oFileCache == null)
					m_oFileCache = new HmrcFileCache();

				return m_oFileCache;
			} // get
		} // HmrcFileCache

		private HmrcFileCache m_oFileCache;

		#endregion property FileCache

		#region method Run

		public override void Run() {
			base.Run();

			if (!string.IsNullOrWhiteSpace(FileCache.ErrorMsg))
				return;

			if (FileCache.AddedCount < 2)
				return;

			FileCache.DateIntervals.Sort((a, b) => a.Left.CompareTo(b.Left));

			DateInterval next = null;

			foreach (DateInterval cur in FileCache.DateIntervals) {
				if (next == null) {
					next = cur;
					continue;
				} // if

				DateInterval prev = next;
				next = cur;

				if (prev.IsJustBefore(next))
					continue;

				FileCache.SetError("Inconsequent date ranges: " + prev + " and " + next);
				return;
			} // for each interval
		} // Run

		#endregion method Run
	} // LocalHmrcFileProcessor

	#endregion class LocalHmrcFileProcessor
} // namespace EzBob.Web.Areas.Customer.Controllers
