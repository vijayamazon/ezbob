namespace Ezbob.Utils {
	using Ezbob.Logger;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;

	public class PostcodeToRegion : SafeLog {

		public PostcodeToRegion(ASafeLog oLog = null) : base(oLog) {
			if (ms_oPostcodeToRegion != null)
				return;

			Init(this);
		} // constructor

		public string this[string sPostCode] {
			get {
				if (sPostCode == null)
					return string.Empty;

				string key = sPostCode.Trim().ToUpper();

				lock (typeof(PostcodeToRegion)) {
					if (ms_oPostcodeToRegion.ContainsKey(key))
						return ms_oPostcodeToRegion[key];
				} // lock

				return string.Empty;
			} // get
		} // indexer

		private static void Init(SafeLog oLog) {
			lock (typeof(PostcodeToRegion)) {
				oLog.Msg("Initialising UK postcode to region mapping...");

				ms_oPostcodeToRegion = new SortedDictionary<string, string>();

				string[] aryLines = new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream("Ezbob.Utils.uk.postcode.regions.txt")
				).ReadToEnd().Split('\n');

				oLog.Debug("{0} rows read from source file.", aryLines.Length);

				string sRegion = string.Empty;

				foreach (string sLine in aryLines) {
					string sCurLine = sLine.Trim();

					if (string.IsNullOrWhiteSpace(sCurLine) || sCurLine.StartsWith("#"))
						continue;

					int nPos = sCurLine.IndexOf('=');

					if (nPos < 0)
						sRegion = sCurLine;
					else {
						string sPostcode = sCurLine.Substring(0, nPos).Trim();

						if (sPostcode != string.Empty && sRegion != string.Empty) {
							string sNormalPostcode = sPostcode.ToUpper();

							ms_oPostcodeToRegion[sNormalPostcode] = sRegion;
							oLog.Debug("UK postcode {0} -> {1} region", sNormalPostcode,  sRegion);
						} // if
					} // if
				} // for each line

				oLog.Msg("Initialising UK postcode to region mapping complete.");
			} // lock
		} // Init

		private static SortedDictionary<string, string> ms_oPostcodeToRegion;

	} // class PostcodeToRegion
} // namespace
