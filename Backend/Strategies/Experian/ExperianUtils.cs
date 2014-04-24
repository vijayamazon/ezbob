namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Xml;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.XmlUtils;

	#region class ExperianUtils

	class ExperianUtils {
		#region constructor

		public ExperianUtils(ASafeLog oLog) {
			Log = new SafeLog(oLog);
		} // constructor

		#endregion constructor

		#region method DetectAml

		public int DetectAml(string sAmlData) {
			XmlNode aml;

			try {
				aml = Xml.ParseRoot(sAmlData);
			}
			catch (SeldenException e) {
				Log.Alert(e, "Could not parse AML data.");
				return 0;
			} // try

			var oPath = new NameList("ProcessConfigResultsBlock", "EIAResultBlock", "AuthenticationIndex");

			XmlNode oNode = aml.Offspring(oPath);

			if (ReferenceEquals(oNode, null)) {
				Log.Alert("Could not find company score tag {0} in AML data.", oPath);
				return 0;
			} // if

			int nScore;

			if (!int.TryParse(oNode.InnerText, out nScore)) {
				Log.Alert("Failed to parse company score content '{0}' as int.", oNode.InnerText);
				return 0;
			} // if

			Log.Debug("AML score is {0}.", nScore);

			return nScore;
		} // DetectAml

		#endregion method DetectAml

		#region method IsDirector

		public bool IsDirector(XmlNode oCompanyInfo, string sFirstName, string sLastName) {
			var oLust = oCompanyInfo.SelectNodes("./REQUEST/DL72");

			if (ReferenceEquals(oLust, null)) {
				Log.Debug("Experian did not provide any director information.");
				return false;
			} // if

			sFirstName = sFirstName.Trim().ToLower();
			sLastName = sLastName.Trim().ToLower();

			foreach (XmlNode oNode in oLust) {
				XmlNode oFirstName = oNode["DIRFORENAME"];

				if (ReferenceEquals(oFirstName, null))
					continue;

				XmlNode oLastName = oNode["DIRSURNAME"];

				if (ReferenceEquals(oLastName, null))
					continue;

				if ((oFirstName.InnerText.Trim().ToLower() == sFirstName) && (oLastName.InnerText.Trim().ToLower() == sLastName)) {
					Log.Debug("The customer is confirmed as a director of this company.");
					return true;
				} // if
			} // for each director

			Log.Debug("Customer name was not found in director list.");
			return false;
		} // IsDirector

		#endregion method IsDirector

		#region method DetectTangibleEquity

		public void DetectTangibleEquity(XmlNode oCompanyInfo, out decimal nResultTangibleEquity, out decimal nResultTotalCurrentAssets) {
			nResultTangibleEquity = -1;
			nResultTotalCurrentAssets = 0;

			decimal nTangibleEquity = -1m;
			decimal nTotalCurrentAssets = 0;

			var oLust = oCompanyInfo.SelectNodes("./REQUEST/DL99");

			if (ReferenceEquals(oLust, null)) {
				Log.Debug("QuickOffer.Validate: Experian did not provide Financial Details IFRS & UK GAAP (DL99).");
				return;
			} // if

			XmlNode oCurNode = null;
			DateTime? oCurDate = null;

			foreach (XmlNode oNode in oLust) {
				DateTime? oDate = ExtractDate(oNode, "DATEOFACCOUNTS", "accounts date");

				if (!oDate.HasValue)
					continue;

				if (ReferenceEquals(oCurNode, null) || (oCurDate.Value < oDate.Value)) {
					oCurNode = oNode;
					oCurDate = oDate;
				} // if
			} // for each DL99

			if (ReferenceEquals(oCurNode, null)) {
				Log.Debug("QuickOffer.Validate: Financial Details IFRS & UK GAAP (DL99) not found.");
				return;
			} // if

			Log.Debug("Calculating tangible equity from data for {0}.", oCurDate.Value.ToString("MMMM d yyyy", CultureInfo.InvariantCulture));

			nTangibleEquity = 0;

			Action<decimal> oPlus = (x) => nTangibleEquity += x;
			Action<decimal> oMinus = (x) => nTangibleEquity -= x;
			Action<decimal> oSet = (x) => nTotalCurrentAssets = x;

			var oTags = new List<Tuple<string, Action<decimal>>> {
				new Tuple<string, Action<decimal>>("TOTALSHAREFUND", oPlus),
				new Tuple<string, Action<decimal>>("INTNGBLASSETS", oMinus),
				new Tuple<string, Action<decimal>>("FINDIRLOANS", oPlus),
				new Tuple<string, Action<decimal>>("CREDDIRLOANS", oPlus),
				new Tuple<string, Action<decimal>>("FINLBLTSDIRLOANS", oPlus),
				new Tuple<string, Action<decimal>>("DEBTORSDIRLOANS", oMinus),
				new Tuple<string, Action<decimal>>("ONCLDIRLOANS", oPlus),
				new Tuple<string, Action<decimal>>("CURRDIRLOANS", oMinus),
				new Tuple<string, Action<decimal>>("TOTALCURRASSETS", oSet),
			};

			var ci = new CultureInfo("en-GB", false);

			foreach (var oTag in oTags) {
				string sTagName = oTag.Item1;
				Action<decimal> oOperation = oTag.Item2;

				XmlNode oNum = oCurNode[sTagName];

				decimal nValue;

				if (ReferenceEquals(oNum, null)) {
					Log.Debug("Tag {0} not found, using 0.", sTagName);
					nValue = 0;
				}
				else {
					if (decimal.TryParse(oNum.InnerText.Trim(), out nValue))
						Log.Debug("{0} = {1}", sTagName, nValue.ToString("C2", ci));
					else
						Log.Debug("Failed to parse tag {0} = '{1}', using 0.", sTagName, oNum.InnerText);
				} // if

				oOperation(nValue);
			} // foreach

			Log.Debug("Tangible equity is {0}.", nTangibleEquity.ToString("C2", ci));
			Log.Debug("Total current assets is {0}.", nTotalCurrentAssets.ToString("C2", ci));

			nResultTangibleEquity = nTangibleEquity;
			nResultTotalCurrentAssets = nTotalCurrentAssets;
		} // DetectTangibleEquity

		#endregion method DetectTangibleEquity

		#region method DetectIncorporationDate

		public DateTime? DetectIncorporationDate(XmlNode oCompanyInfo) {
			var oPath = new NameList("REQUEST", "DL12");

			XmlNode oNode = oCompanyInfo.Offspring(oPath);

			if (ReferenceEquals(oNode, null)) {
				Log.Alert("Could not find incorporation date container tag {0} in company data.", oPath);
				return null;
			} // if

			DateTime? oDate = ExtractDate(oNode, "DATEINCORP", "incorporation date");

			if (!oDate.HasValue)
				return null;

			Log.Debug("Incorporation date is {0}; current age in days is {1:N}.", oDate.Value.ToString("MMMM d yyyy"), DateTime.UtcNow.Subtract(oDate.Value).TotalDays);
			return oDate;
		} // DetectIncorporationDate

		#endregion method DetectIncorporationDate

		#region method DetectBusinessScore

		public int DetectBusinessScore(XmlNode oCompanyInfo) {
			var oPath = new NameList("REQUEST", "DL76", "RISKSCORE");

			XmlNode oNode = oCompanyInfo.Offspring(oPath);

			if (ReferenceEquals(oNode, null)) {
				Log.Alert("Could not find business score tag {0} in company data.", oPath);
				return 0;
			} // if

			int nScore;

			if (!int.TryParse(oNode.InnerText, out nScore)) {
				Log.Alert("Failed to parse business score content '{0}' as int.", oNode.InnerText);
				return 0;
			} // if

			Log.Debug("Business score is {0}.", nScore);

			return nScore;
		} // DetectBusinessScore

		#endregion method DetectBusinessScore

		#region private

		#region method ExtractDate

		private DateTime? ExtractDate(XmlNode oParent, string sBaseTagName, string sDateDisplayName) {
			XmlNode oYear = oParent[sBaseTagName + "-YYYY"];

			if (ReferenceEquals(oYear, null)) {
				Log.Alert("Could not find {0} year tag.", sDateDisplayName);
				return null;
			} // if

			XmlNode oMonth = oParent[sBaseTagName + "-MM"];

			if (ReferenceEquals(oMonth, null)) {
				Log.Alert("Could not find {0} month tag.", sDateDisplayName);
				return null;
			} // if

			XmlNode oDay = oParent[sBaseTagName + "-DD"];

			if (ReferenceEquals(oDay, null)) {
				Log.Alert("Could not find {0} day tag.", sDateDisplayName);
				return null;
			} // if

			string sDate = string.Format(
				"{0}-{1}-{2}",
				oYear.InnerText.Trim().PadLeft(4, '0'),
				oMonth.InnerText.Trim().PadLeft(2, '0'),
				oDay.InnerText.Trim().PadLeft(2, '0')
			);

			DateTime oDate;

			if (!DateTime.TryParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out oDate)) {
				Log.Alert("Could not find parse {1} from '{0}'.", sDate, sDateDisplayName);
				return null;
			} // if

			return oDate;
		} // ExtractDate

		#endregion method ExtractDate

		private SafeLog Log { get; set; } // Log

		#endregion private
	} // class ExperianUtils

	#endregion class ExperianUtils
} // namespace EzBob.Backend.Strategies
