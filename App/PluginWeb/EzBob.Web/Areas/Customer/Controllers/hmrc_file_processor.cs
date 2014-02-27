namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using System.Web;
	using EZBob.DatabaseLib;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;
	using log4net;

	#region class AHmrcFileProcessor

	internal abstract class AHmrcFileProcessor {
		#region public

		#region opertor cast to HmrcController.ValidateFilesResult

		public static implicit operator HmrcController.ValidateFilesResult(AHmrcFileProcessor hfp) {
			return new HmrcController.ValidateFilesResult {
				Hopper = hfp.Hopper,
				Error = hfp.Error,
			};
		} // operator cast to HmrcController.ValidateFilesResult

		#endregion opertor cast to HmrcController.ValidateFilesResult

		public abstract int AddedCount { get; protected set; } // AddedCount
		public abstract Hopper Hopper { get; } // Hopper

		#region property Error

		public virtual string Error { get; protected set; } // Error

		#endregion property Error

		#region method Run

		public virtual void Run() {
			Error = null;

			AddedCount = 0;

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

				SaveToDisc(oFile.FileName, oFileContents);

				var smd = new SheafMetaData {
					BaseFileName = oFile.FileName,
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null
				};

				Hopper.Add(smd, oFileContents);

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
						CleanupOnError("Inconsistent business registration number.");
						return;
					} // if
				}
				else
					nRegistrationNo = oSeeds.RegistrationNo;

				var di = new DateInterval(oSeeds.DateFrom, oSeeds.DateTo);

				Log.DebugFormat("There are {0} insterval(s) in DateIntervals.", DateIntervals.Count);

				foreach (DateInterval oInterval in DateIntervals) {
					if (oInterval.Intersects(di)) {
						CleanupOnError("Inconsistent date ranges: " + oInterval + " and " + di);
						return;
					} // if
				} // for each

				DateIntervals.Add(di);
				Hopper.Add(smd, oResult);
				AddedCount++;
			} // for
		} // Run

		#endregion method Run

		#endregion public

		#region protected

		#region constructor

		protected AHmrcFileProcessor(int nCustomerID, HttpFileCollectionBase oFiles) {
			CustomerID = nCustomerID;
			FileList = oFiles;
			Error = null;
		} // constructor

		#endregion constructor

		protected static readonly ILog Log = LogManager.GetLogger(typeof(AHmrcFileProcessor));

		protected abstract List<DateInterval> DateIntervals { get; } // DateIntervals

		#endregion protected

		#region private

		private HttpFileCollectionBase FileList { get; set; } // FileList
		private int CustomerID { get; set; } // CustomerID

		#region method CleanupOnError

		private void CleanupOnError(string sErrorMsg) {
			Hopper.Clean();
			AddedCount = 0;

			Error = sErrorMsg;
			Log.WarnFormat("Hopper and AddedCount has been cleaned because of error: {0}", Error);
		} // CleanupOnError

		#endregion method CleanupOnError

		#region method SaveToDisc

		private void SaveToDisc(string sFileOriginalName, byte[] oFileContents) {
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
						string sFileName = Path.Combine(sPath, Guid.NewGuid().ToString("N") + "." + CustomerID + "." + sFileOriginalName);

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
		#region public

		#region constructor

		public SessionHmrcFileProcessor(HttpSessionStateBase oSession, int nCustomerID, HttpFileCollectionBase oFiles) : base(nCustomerID, oFiles) {
			Session = oSession;
		} // constructor

		#endregion constructor

		#region property AddedCount

		public override int AddedCount {
			get {
				if (Session["AddedCount"] == null)
					Session["AddedCount"] = 0;

				return (int)Session["AddedCount"];
			} // get

			protected set {
				Session["AddedCount"] = value;
			} // set
		} // AddedCount

		#endregion property AddedCount

		#region property Hopper

		public override Hopper Hopper {
			get {
				if (Session["Hopper"] == null)
					Session["Hopper"] = new Hopper();

				return Session["Hopper"] as Hopper;
			} // get
		} // Hopper

		#endregion property Hopper

		#endregion public

		#region protected

		#region property DateIntervals

		protected override List<DateInterval> DateIntervals {
			get {
				if (Session["DateIntervals"] == null)
					Session["DateIntervals"] = new List<DateInterval>();

				return Session["DateIntervals"] as List<DateInterval>;
			} // get
		} // DateIntervals

		#endregion property DateIntervals

		#endregion protected

		#region private

		private HttpSessionStateBase Session { get; set; } // Session

		#endregion private
	} // SessionHmrcFileProcessor

	#endregion class SessionHmrcFileProcessor

	#region class LocalHmrcFileProcessor

	internal class LocalHmrcFileProcessor : AHmrcFileProcessor {
		#region public

		#region constructor

		public LocalHmrcFileProcessor(int nCustomerID, HttpFileCollectionBase oFiles) : base(nCustomerID, oFiles) {
		} // constructor

		#endregion constructor

		#region property AddedCount

		public override int AddedCount { get; protected set; } // AddedCount

		#endregion property AddedCount

		#region property Hopper

		public override Hopper Hopper {
			get {
				if (m_oHopper == null)
					m_oHopper = new Hopper();

				return m_oHopper;
			} // get
		} // Hopper

		private Hopper m_oHopper;

		#endregion property Hopper

		#region method Run

		public override void Run() {
			base.Run();

			ValidateDateConsequency();
		} // Run

		#endregion method Run

		#endregion public

		#region protected

		#region property DateIntervals

		protected override List<DateInterval> DateIntervals {
			get {
				if (m_oDateIntervals == null)
					m_oDateIntervals = new List<DateInterval>();

				return m_oDateIntervals;
			} // get
		} // DateIntervals

		private List<DateInterval> m_oDateIntervals;

		#endregion property DateIntervals

		#endregion protected

		#region private

		#region method ValidateDateConsequency

		private void ValidateDateConsequency() {
			if (Error != null)
				return;

			if (AddedCount < 2)
				return;

			DateIntervals.Sort((a, b) => a.Left.CompareTo(b.Left));

			DateInterval next = null;

			foreach (DateInterval cur in DateIntervals) {
				if (next == null) {
					next = cur;
					continue;
				} // if

				DateInterval prev = next;
				next = cur;

				if (!prev.IsJustBefore(next)) {
					Error = "Inconsequent date ranges: " + prev + " and " + next;
					return;
				} // if
			} // for each interval
		} // ValidateDateConsequency

		#endregion method ValidateDateConsequency

		#endregion private
	} // LocalHmrcFileProcessor

	#endregion class LocalHmrcFileProcessor
} // namespace EzBob.Web.Areas.Customer.Controllers
