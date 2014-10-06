﻿namespace ConfigManager {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Logger;
	using Ezbob.Utils.MimeTypes;

	public class OneUploadLimitation {
		#region constructor

		public OneUploadLimitation() : this(0, "") {} // constructor

		public OneUploadLimitation(int nFileSize, string sAcceptedFiles) {
			m_oMimeTypesFromExtensions = new SortedSet<string>();

			Init(nFileSize, sAcceptedFiles);
		} // constructor

		#endregion constructor

		#region method Init

		public void Init(int nFileSize, string sAcceptedFiles) {
			FileSize = nFileSize;

			MimeTypePrefixes = new SortedSet<string>();
			MimeTypes = new SortedSet<string>();
			FileExtensions = new SortedSet<string>();
			m_oMimeTypesFromExtensions.Clear();

			var mtr = new MimeTypeResolver();

			if (sAcceptedFiles == Star)
				AcceptedFiles = Star;
			else {
				if (!string.IsNullOrWhiteSpace(sAcceptedFiles)) {
					foreach (var s in sAcceptedFiles.Split(',')) {
						var sItem = s.Trim();

						if (string.IsNullOrWhiteSpace(s))
							continue;

						if (sItem.StartsWith(".")) {
							FileExtensions.Add(sItem);
							m_oMimeTypesFromExtensions.Add(mtr[sItem]);
							continue;
						} // if

						int nPos = sItem.IndexOf(StarChar);

						if (nPos < 0)
							MimeTypes.Add(sItem);
						else
							MimeTypePrefixes.Add(sItem.Substring(0, nPos));
					} // for each
				} // if

				List<string> oAccepted = new List<string>();

				oAccepted.AddRange(MimeTypePrefixes.Select(s => s + Star));
				oAccepted.AddRange(MimeTypes);
				oAccepted.AddRange(FileExtensions);

				AcceptedFiles = string.Join(",", oAccepted);
			} // if
		} // Init

		#endregion method Init

		#region property FileSize

		public int FileSize { get; private set; }

		#endregion property FileSize

		#region property AcceptedFiles

		public string AcceptedFiles { get; private set; }

		#endregion property AcceptedFiles

		#region property MimeTypePrefixes

		public SortedSet<string> MimeTypePrefixes { get; private set; }

		#endregion property MimeTypePrefixes

		#region property MimeTypes

		public SortedSet<string> MimeTypes { get; private set; }

		#endregion property MimeTypes

		#region property FileExtensions

		public SortedSet<string> FileExtensions { get; private set; }

		#endregion property FileExtensions

		#region method ToString

		public override string ToString() {
			return FileSize.ToString("N0", CultureInfo.InvariantCulture) + " bytes of " + AcceptedFiles;
		} // ToString

		#endregion method ToString

		#region method DetectFileMimeType

		public string DetectFileMimeType(byte[] oFilePrefix, string sFileName, int? nFilePrefixLength = 256, ASafeLog oLog = null) {
			oLog = oLog ?? new SafeLog();

			var mtr = new MimeTypeResolver();

			MimeType oExtMimeType = mtr.Get(sFileName);

			if (oExtMimeType != null) {
				oLog.Debug("MIME type by file extension is {0}.", oExtMimeType.PrimaryMimeType);

				if (!this.Contains(oExtMimeType.PrimaryMimeType)) {
					oLog.Debug("MIME type by file extension {0} does not conform to this limitation {1}.", oExtMimeType.PrimaryMimeType, this);
					return null;
				} // if
			}
			else
				oLog.Debug("MIME type cannot be detected by file name {0}.", sFileName);

			if (oFilePrefix != null) {
				string sFileMimeType = mtr.GetFromFile(oFilePrefix, nFilePrefixLength);

				oLog.Debug("MIME type by content is {0}", sFileMimeType);

				if (this.Contains(sFileMimeType)) {
					oLog.Debug("MIME type by content {0} conforms to this limitation {1}.", sFileMimeType, this);
					return sFileMimeType;
				} // if

				oLog.Debug("MIME type by content {0} does not conform to this limitation {1}.", sFileMimeType, this);

				MimeType oFileMimeType = mtr.Find(sFileMimeType);

				if (oFileMimeType == null) {
					oFileMimeType = MimeType.Binary;
					oLog.Debug("Could not find MIME type by its name '{0}', using default MIME type '{1}'.", sFileMimeType, oFileMimeType.PrimaryMimeType);
				} // if

				if ((oExtMimeType == null) && oFileMimeType.IsCommon) {
					oLog.Debug(
						"MIME type by file ('{0}') is a common one and MIME type by extension cannot be detected - file should be dropped.",
						oFileMimeType.PrimaryMimeType
					);
					return null;
				} // if

				if (oFileMimeType * oExtMimeType) {
					oLog.Debug(
						"MIME type by file ('{0}') differs from MIME type by extension ('{1}') but they are compatible so using the latter.",
						oFileMimeType.PrimaryMimeType,
						oExtMimeType == null ? "-- null --" : oExtMimeType.PrimaryMimeType
						);

					return oExtMimeType == null ? null : oExtMimeType.PrimaryMimeType;
				} // if

				oLog.Debug(
					"MIME type by file ('{0}') differs from MIME type by extension ('{1}') and they are not compatible.",
					oFileMimeType.PrimaryMimeType,
					oExtMimeType == null ? string.Empty : oExtMimeType.PrimaryMimeType
				);

				return null;
			} // if

			if (oExtMimeType != null) {
				oLog.Debug("MIME type by file extension {0} conforms to this limitation {1}.", oExtMimeType.PrimaryMimeType, this);
				return oExtMimeType.PrimaryMimeType;
			} // if

			oLog.Debug("No MIME type detected, returning null.");
			return null;
		} // DetectFileMimeType

		#endregion method DetectFileMimeType

		#region private

		private const string Star = "*";
		private const char StarChar = '*';
		private readonly SortedSet<string> m_oMimeTypesFromExtensions;

		#region method Contains

		private bool Contains(string sMimeType) {
			if (MimeTypes.Contains(sMimeType) || m_oMimeTypesFromExtensions.Contains(sMimeType))
				return true;

			foreach (var s in MimeTypePrefixes)
				if (sMimeType.StartsWith(s))
					return true;

			return false;
		} // Contains

		#endregion method Contains

		#endregion private
	} // class OneUploadLimitation
} // namespace
