using System;
using System.Globalization;
using Ezbob.Logger;

namespace Ezbob.HmrcHarvester {

	public abstract class AThrasher : SafeLog {

		private const string PoundStr = "£";
		private const char PoundChar = '£';

		public static decimal ParseGBP(string sMoney) {
			// Convert unicode pound sign to ascii.
			string sValue = sMoney
				.Replace(Convert.ToChar(65533), PoundChar)
				.Replace("GBP", PoundStr);

			return decimal.Parse(sValue, NumberStyles.Currency, Culture);
		} // ParseGBP

		public abstract ISeeds Run(SheafMetaData oFileID, byte[] oFile);

		protected AThrasher(bool bVerboseLogging = false, ASafeLog oLog = null) : base(oLog) {
			VerboseLogging = bVerboseLogging;
		} // constructor

		protected bool VerboseLogging { get; private set; }

		protected static CultureInfo Culture {
			get {
				if (ms_oCulture == null)
					ms_oCulture = new CultureInfo("en-GB", false);

				return ms_oCulture;
			} // get
		} // Culture

		private static CultureInfo ms_oCulture;

		public abstract ISeeds Seeds { get; protected set; }

	} // class AThrasher

} // namespace Ezbob.HmrcHarvester
