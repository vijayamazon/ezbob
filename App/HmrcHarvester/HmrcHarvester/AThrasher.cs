using System;
using System.Globalization;
using Ezbob.Logger;

namespace Ezbob.HmrcHarvester {
	#region class AThrasher

	public abstract class AThrasher : SafeLog {
		#region public

		#region method ParseGBP

		public static decimal ParseGBP(string sMoney) {
			var ci = new CultureInfo("en-GB", false);
			
			// Convert unicode pound sign to ascii.
			string sValue = sMoney.Replace(Convert.ToChar(65533), Convert.ToChar(163));

			return decimal.Parse(sValue, NumberStyles.Currency, ci);
		} // ParseGBP

		#endregion method ParseGBP

		public abstract ISeeds Run(SheafMetaData oFileID, byte[] oFile);

		#endregion public

		#region protected

		#region constructor

		protected AThrasher(ASafeLog oLog = null) : base(oLog) {
		} // constructor

		#endregion constructor

		#endregion protected
	} // class AThrasher

	#endregion class AThrasher
} // namespace Ezbob.HmrcHarvester
