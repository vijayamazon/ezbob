namespace EzBob.Backend.Strategies.QuickOffer {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.XmlUtils;
	using Models;

	#region class QuickOfferData

	internal class QuickOfferData {
		#region public

		#region constructor

		public QuickOfferData(ASafeLog oLog) {
			Log = new SafeLog(oLog);
			IsValid = false;
		} // constructor

		#endregion constructor

		#region method Load

		public void Load(SafeReader oReader) {
			CustomerID = oReader["CustomerID"];
			RequestedAmount = oReader["RequestedAmount"];
			// IsOffline = oReader["IsOffline"];
			CompanyRefNum = oReader["CompanyRefNum"];
			// DefaultCount = oReader["DefaultCount"];
			// AmlID = oReader["AmlID"];
			AmlData = oReader["AmlData"];
			// PersonalID = oReader["PersonalID"];
			// PersonalScore = oReader["PersonalScore"];
			// CompanyID = oReader["CompanyID"];
			CompanyData = oReader["CompanyData"];
			FirstName = oReader["FirstName"];
			LastName = oReader["LastName"];

			Validate();
		} // Load

		#endregion method Load

		#region property IsValid

		public bool IsValid { get; private set; } // IsValid

		#endregion property IsValid

		#region method GetOffer

		public QuickOfferModel GetOffer(bool bSaveOfferToDB, AConnection oDB, StrategyLog oLog) {
			decimal? nOffer = Calculate();

			if (!nOffer.HasValue)
				return null;

			int nOfferID = default (int);

			if (bSaveOfferToDB) {
				try {
					var oID = new QueryParameter("@QuickOfferID") {
						Type = SqlDbType.Int,
						Direction = ParameterDirection.Output,
					};

					oDB.ExecuteNonQuery(
						"QuickOfferSave",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@CustomerID", CustomerID),
						new QueryParameter("@Amount", nOffer),
						new QueryParameter("@Aml", Aml),
						new QueryParameter("@BusinessScore", BusinessScore),
						new QueryParameter("@IncorporationDate", IncorporationDate),
						new QueryParameter("@TangibleEquity", TangibleEquity),
						new QueryParameter("@TotalCurrentAssets", TotalCurrentAssets),
						oID
					);

					if (int.TryParse(oID.SafeReturnedValue, out nOfferID))
						oLog.Msg("Quick offer id is {0}", nOfferID);
					else
						oLog.Warn("Failed to parse quick offer id from {0}", oID.Value.ToString());
				}
				catch (Exception e) {
					oLog.Alert("Failed to save a quick offer to DB.", e);
				} // try
			} // if

			return new QuickOfferModel {
				ID = nOfferID,
				Amount = nOffer.Value,
				Aml = Aml,
				BusinessScore = BusinessScore,
				IncorporationDate = IncorporationDate,
				TangibleEquity = TangibleEquity,
				TotalCurrentAssets = TotalCurrentAssets,
			};
		} // GetOffer

		#endregion method GetOffer

		#endregion public

		#region private

		#region properties

		private int CustomerID;
		private decimal RequestedAmount;
		// private bool IsOffline;
		private string CompanyRefNum;
		// private int DefaultCount;
		// private long AmlID;
		private string AmlData;
		// private long PersonalID;
		// private int PersonalScore;
		// private long CompanyID;
		private string CompanyData;
		private string FirstName;
		private string LastName;

		private int Aml;
		private int BusinessScore;
		private DateTime IncorporationDate;
		private decimal TangibleEquity;
		private decimal TotalCurrentAssets;

		private readonly SafeLog Log;

		#endregion properties

		#region method Calculate

		private decimal? Calculate() {
			var oOfferAmountPct = new List<Tuple<int, decimal>>() {
				new Tuple<int, decimal>(40, 0.012m),
				new Tuple<int, decimal>(50, 0.016m),
				new Tuple<int, decimal>(60, 0.027m),
				new Tuple<int, decimal>(70, 0.031m),
				new Tuple<int, decimal>(80, 0.034m),
				new Tuple<int, decimal>(90, 0.051m),
				new Tuple<int, decimal>(100, 0.069m),
			};

			decimal nPct = 0;

			foreach (var oPct in oOfferAmountPct) {
				if (BusinessScore <= oPct.Item1) {
					nPct = oPct.Item2;
					break;
				} // if
			} // foreach

			if (nPct == 0) {
				Log.Debug("No percent found to business score {0}.", BusinessScore);
				return null;
			} // if

			Log.Debug("Percent for business score {0} is {1}.", BusinessScore, nPct.ToString("P2"));

			decimal nCalculatedOffer = TotalCurrentAssets * nPct;

			var ci = new CultureInfo("en-GB", false);

			Log.Debug("Calculated offer (total current assets * percent) is {0}", nCalculatedOffer.ToString("C2", ci));

			nCalculatedOffer = Math.Truncate(nCalculatedOffer / 100.0m) * 100.0m;

			Log.Debug("Rounded offer is {0}", nCalculatedOffer.ToString("C2", ci));

			if (nCalculatedOffer < 1000.0m) {
				Log.Debug("The offer is less than {0}, not offering.", (1000.0m).ToString("C2", ci));
				return null;
			} // if

			decimal n15pct = RequestedAmount * 0.15m;
			n15pct = Math.Truncate(n15pct / 100.0m) * 100.0m;

			decimal nCap = Math.Min(7000.0m, n15pct);

			Log.Debug(
				"Offer cap is {0} = min({1}, {2} * 15% = {3})",
				nCap.ToString("C2", ci),
				(7000.0m).ToString("C2", ci),
				RequestedAmount.ToString("C2", ci),
				n15pct.ToString("C2", ci)
			);

			decimal nOffer = Math.Min(nCalculatedOffer, nCap);

			Log.Debug(
				"And the offer is {0} = min({1}, {2})",
				nOffer.ToString("C2", ci),
				nCalculatedOffer.ToString("C2", ci),
				nCap.ToString("C2", ci)
			);

			return nOffer;
		} // Calculate

		#endregion method Calculate

		#region method Validate

		private void Validate() {
			if (!AreLoadedValid())
				return;

			DetectAml();
			if (Aml <= 70) {
				Log.Debug("QuickOffer.Validate: AML is too low.");
				return;
			} // if

			XmlNode oCompanyInfo = Xml.ParseRoot(CompanyData);

			if (!IsDirector(oCompanyInfo)) {
				Log.Debug("QuickOffer.Validate: the customer is not director of this company.");
				return;
			} // if

			DetectBusinessScore(oCompanyInfo);
			if (BusinessScore < 31) {
				Log.Debug("QuickOffer.Validate: business score is too low.");
				return;
			} // if

			DetectIncorporationDate(oCompanyInfo);
			if (DateTime.UtcNow.Subtract(IncorporationDate).TotalDays < 365 * 3) {
				Log.Debug("QuickOffer.Validate: business is too young.");
				return;
			} // if

			DetectTangibleEquity(oCompanyInfo);
			if (TangibleEquity < 0) {
				Log.Debug("QuickOffer.Validate: tangible equity is to low.");
				return;
			} // if

			IsValid = true;
		} // Validate

		#endregion method Validate

		#region method DetectIncorporationDate

		private void DetectIncorporationDate(XmlNode oCompanyInfo) {
			var oPath = new NameList("REQUEST", "DL12");

			XmlNode oNode = oCompanyInfo.Offspring(oPath);

			if (ReferenceEquals(oNode, null)) {
				Log.Alert("Could not find incorporation date container tag {0} in company data.", oPath);
				return;
			} // if

			DateTime? oDate = ExtractDate(oNode, "DATEINCORP", "incorporation date");

			if (!oDate.HasValue)
				return;

			IncorporationDate = oDate.Value;
			Log.Debug("Incorporation date is {0}; current age in days is {1:N}.", IncorporationDate.ToString("MMMM d yyyy"), DateTime.UtcNow.Subtract(IncorporationDate).TotalDays);
		} // DetectIncorporationDate

		#endregion method DetectIncorporationDate

		#region method DetectTangibleEquity

		private void DetectTangibleEquity(XmlNode oCompanyInfo) {
			TangibleEquity = -1;
			TotalCurrentAssets = 0;

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

			Action<decimal> oPlus = (x) => TangibleEquity += x;
			Action<decimal> oMinus = (x) => TangibleEquity -= x;
			Action<decimal> oSet = (x) => TotalCurrentAssets = x;

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

			TangibleEquity = 0;

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

			Log.Debug("Tangible equity is {0}.", TangibleEquity.ToString("C2", ci));
			Log.Debug("Total current assets is {0}.", TotalCurrentAssets.ToString("C2", ci));
		} // DetectTangibleEquity

		#endregion method DetectTangibleEquity

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

		#region method DetectBusinessScore

		private void DetectBusinessScore(XmlNode oCompanyInfo) {
			var oPath = new NameList("REQUEST", "DL76", "RISKSCORE");

			XmlNode oNode = oCompanyInfo.Offspring(oPath);

			if (ReferenceEquals(oNode, null)) {
				Log.Alert("Could not find business score tag {0} in company data.", oPath);
				return;
			} // if

			int nScore;

			if (!int.TryParse(oNode.InnerText, out nScore)) {
				Log.Alert("Failed to parse business score content '{0}' as int.", oNode.InnerText);
				return;
			} // if

			BusinessScore = nScore;

			Log.Debug("Business score is {0}.", BusinessScore);
		} // DetectBusinessScore

		#endregion method DetectBusinessScore

		#region method AreLoadedValid

		private bool AreLoadedValid() {
			if (CustomerID == 0) {
				Log.Debug("QuickOffer.Validate: customer id not set.");
				return false;
			} // if

			if (RequestedAmount < 0) {
				Log.Debug("QuickOffer.Validate: requested amount is not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(CompanyRefNum)) {
				Log.Debug("QuickOffer.Validate: company ref number not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(AmlData)) {
				Log.Debug("QuickOffer.Validate: AML data not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(CompanyData)) {
				Log.Debug("QuickOffer.Validate: company Experian data not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(FirstName)) {
				Log.Debug("QuickOffer.Validate: customer first name not set.");
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(LastName)) {
				Log.Debug("QuickOffer.Validate: customer last name not set.");
				return false;
			} // if

			return true;
		} // AreLoadedValid

		#endregion method AreLoadedValid

		#region method IsDirector

		private bool IsDirector(XmlNode oCompanyInfo) {
			var oLust = oCompanyInfo.SelectNodes("./REQUEST/DL72");

			if (ReferenceEquals(oLust, null)) {
				Log.Debug("QuickOffer.Validate: Experian did not provide any director information.");
				return false;
			} // if

			string sFirstName = FirstName.Trim().ToLower();
			string sLastName = LastName.Trim().ToLower();

			foreach (XmlNode oNode in oLust) {
				XmlNode oFirstName = oNode["DIRFORENAME"];

				if (ReferenceEquals(oFirstName, null))
					continue;

				XmlNode oLastName = oNode["DIRSURNAME"];

				if (ReferenceEquals(oLastName, null))
					continue;

				if ((oFirstName.InnerText.Trim().ToLower() == sFirstName) && (oLastName.InnerText.Trim().ToLower() == sLastName)) {
					Log.Debug("QuickOffer.Validate: the customer is confirmed as a director of this company.");
					return true;
				} // if
			} // for each director

			Log.Debug("QuickOffer.Validate: customer name not found in director list.");
			return false;
		} // IsDirector

		#endregion method IsDirector

		#region method DetectAml

		private void DetectAml() {
			XmlNode aml;

			try {
				aml = Xml.ParseRoot(AmlData);
			}
			catch (SeldenException e) {
				Log.Alert(e, "Could not parse AML data.");
				return;
			} // try

			var oPath = new NameList("ProcessConfigResultsBlock", "EIAResultBlock", "AuthenticationIndex");

			XmlNode oNode = aml.Offspring(oPath);

			if (ReferenceEquals(oNode, null)) {
				Log.Alert("Could not find company score tag {0} in AML data.", oPath);
				return;
			} // if

			int nScore;

			if (!int.TryParse(oNode.InnerText, out nScore)) {
				Log.Alert("Failed to parse company score content '{0}' as int.", oNode.InnerText);
				return;
			} // if

			Aml = nScore;

			Log.Debug("AML score is {0}.", Aml);
		} // DetectAml

		#endregion method DetectAml

		#endregion private
	} // class QuickOfferData

	#endregion class QuickOfferData
} // namespace EzBob.Backend.Strategies.QuickOffer
