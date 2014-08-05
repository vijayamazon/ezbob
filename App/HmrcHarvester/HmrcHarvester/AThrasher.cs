using System;
using System.Globalization;
using Ezbob.Logger;

namespace Ezbob.HmrcHarvester {
	#region class AThrasher

	public abstract class AThrasher : SafeLog {
		#region public

		private const string PoundStr = "£";
		private const char PoundChar = '£';

		#region method ParseGBP

		public static decimal ParseGBP(string sMoney) {
			// Convert unicode pound sign to ascii.
			string sValue = sMoney
				.Replace(Convert.ToChar(65533), PoundChar)
				.Replace("GBP", PoundStr);

			return decimal.Parse(sValue, NumberStyles.Currency, Culture);
		} // ParseGBP

		#endregion method ParseGBP

		public abstract ISeeds Run(SheafMetaData oFileID, byte[] oFile);

		#endregion public

		#region protected

		#region constructor

		protected AThrasher(bool bVerboseLogging = false, ASafeLog oLog = null) : base(oLog) {
			VerboseLogging = bVerboseLogging;
		} // constructor

		#endregion constructor

		#region property VerboseLogging

		protected bool VerboseLogging { get; private set; }

		#endregion property VerboseLogging

		#region property Culture

		protected static CultureInfo Culture {
			get {
				if (ms_oCulture == null)
					ms_oCulture = new CultureInfo("en-GB", false);

				return ms_oCulture;
			} // get
		} // Culture

		private static CultureInfo ms_oCulture;

		#endregion property Culture

		public abstract ISeeds Seeds { get; protected set; }

		#endregion protected

		#region private

		#endregion private
	} // class AThrasher

	#endregion class AThrasher
} // namespace Ezbob.HmrcHarvester
