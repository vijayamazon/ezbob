namespace EzBob.Backend.Strategies.QuickOffer {
	using System;
	using System.Collections.Generic;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.XmlUtils;

	#region class QuickOfferData

	internal class QuickOfferData {
		#region public

		#region constructor

		public QuickOfferData(ASafeLog oLog) {
			Log = new SafeLog(oLog);
			IsValid = false;
		}

		// constructor

		#endregion constructor

		#region method Load

		public void Load(SafeReader oReader) {
			CustomerID = oReader["CustomerID"];
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
		}

		// Load

		#endregion method Load

		#region property IsValid

		public bool IsValid { get; private set; } // IsValid

		#endregion property IsValid

		#region method Calculate

		public decimal? Calculate() {
			// TODO
			return null;
		}

		// Calculate

		#endregion method Calculate

		#endregion public

		#region private

		#region properties

		private int CustomerID;
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

		private readonly SafeLog Log;

		#endregion properties

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

			IsValid = true;
		}

		// Validate

		#endregion method Validate

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
				Log.Debug("QuickOffer.Validate: Experian did not private any director information.");
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
