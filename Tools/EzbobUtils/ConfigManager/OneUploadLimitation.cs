namespace ConfigManager {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	public class OneUploadLimitation {
		#region constructor

		public OneUploadLimitation() : this(0, "") {} // constructor

		public OneUploadLimitation(int nFileSize, string sAcceptedFiles) {
			Init(nFileSize, sAcceptedFiles);
		} // constructor

		#endregion constructor

		#region method Init

		public void Init(int nFileSize, string sAcceptedFiles) {
			FileSize = nFileSize;

			MimeTypePrefixes = new SortedSet<string>();
			MimeTypes = new SortedSet<string>();
			FileExtensions = new SortedSet<string>();

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

		private const string Star = "*";
		private const char StarChar = '*';
	} // class OneUploadLimitation
} // namespace
