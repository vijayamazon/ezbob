namespace ExperianLib.Ebusiness {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using Ezbob.Logger;

	public abstract class BusinessReturnData {
		#region public

		public bool IsError {
			get { return !string.IsNullOrEmpty(Error); }
		} // IsError

		public string Error { get; set; }
		public DateTime? LastCheckDate { get; protected set; }
		public bool IsDataExpired { get; set; }
		public string OutputXml { get; private set; }

		public decimal BureauScore { get; set; }
		public decimal MaxBureauScore { get; set; }
		public decimal CreditLimit { get; set; }

		public string CompanyName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string AddressLine4 { get; set; }
		public string AddressLine5 { get; set; }
		public string PostCode { get; set; }

		public DateTime? IncorporationDate { get; protected set; }
		public bool CacheHit { get; set; }

		public abstract bool IsLimited { get; }

		#endregion public

		#region protected

		#region constructors

		protected BusinessReturnData() {
		} // constructor

		protected BusinessReturnData(Exception ex) : this() {
			Error = ex.Message;
		} // constructor

		protected BusinessReturnData(string outputXml, DateTime lastCheckDate) : this() {
			LastCheckDate = lastCheckDate;
			OutputXml = outputXml;

			try {
				XElement root = XDocument.Load(new StringReader(outputXml)).Root;

				IEnumerable<XElement> errors = root.XPathSelectElements("./REQUEST/ERR1/MESSAGE");

				foreach (var el in errors)
					Error += el.Value + Environment.NewLine;

				Parse(root);
			}
			catch {
				Error = "Invalid XML returned from e-series: " + outputXml;
			} // try
		} // constructor

		#endregion constructors

		protected abstract void Parse(XElement root);

		#region method ExtractDate

		protected DateTime? ExtractDate(XElement oParent, string sBaseTagName, string sDateDisplayName, bool bDateOneIfNotFound = false) {
			XElement oYear = oParent.XPathSelectElement("./" + sBaseTagName + "-YYYY");

			if (ReferenceEquals(oYear, null)) {
				ms_oLog.Alert("Could not find {0} year tag.", sDateDisplayName);
				return null;
			} // if

			XElement oMonth = oParent.XPathSelectElement("./" + sBaseTagName + "-MM");

			if (ReferenceEquals(oMonth, null)) {
				ms_oLog.Alert("Could not find {0} month tag.", sDateDisplayName);
				return null;
			} // if

			XElement oDay = oParent.XPathSelectElement("./" + sBaseTagName + "-DD");
			string sDay;

			if (ReferenceEquals(oDay, null)) {
				if (bDateOneIfNotFound)
					sDay = "1";
				else {
					ms_oLog.Alert("Could not find {0} day tag.", sDateDisplayName);
					return null;
				}
			}
			else
				sDay = oDay.Value;

			string sDate = string.Format(
				"{0}-{1}-{2}",
				oYear.Value.Trim().PadLeft(4, '0'),
				oMonth.Value.Trim().PadLeft(2, '0'),
				sDay.Trim().PadLeft(2, '0')
			);

			DateTime oDate;

			if (!DateTime.TryParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out oDate)) {
				ms_oLog.Alert("Could not find parse {1} from '{0}'.", sDate, sDateDisplayName);
				return null;
			} // if

			return oDate;
		} // ExtractDate

		#endregion method ExtractDate

		#endregion protected

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (BusinessReturnData));
	} // class BusinessReturnData
} // namespace
