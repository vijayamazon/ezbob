namespace Ezbob.HmrcHarvester {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Ezbob.Utils.Security;
	using Ezbob.Logger;
	using HtmlAgilityPack;
	using Integration.ChannelGrabberAPI;
	using Integration.ChannelGrabberConfig;
	using DBCustomer = EZBob.DatabaseLib.Model.Database.Customer;
	using Backend.Models;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using StructureMap;

	/// <summary>
	/// A Harvester harvests from the Field (http://hmrc.gov.uk) and stores the harvest (VAT return etc) in the Hopper.
	/// Uses UserName and Password to gain access the Field.
	///
	/// <example>
	/// Harvester harvester = new Harvester();
	///
	/// if (harvester.Init()) {
	///     harvester.Run();
	///
	///     // do something with harvester.Hopper
	///
	/// } // if
	///
	/// harvester.Done();
	///
	/// </example>
	/// </summary>
	public class Harvester : SafeILog, Integration.ChannelGrabberAPI.IHarvester {
		public static void SetBackdoorData(int nCustomerMarketplaceID, Hopper oHopper) {
			if (oHopper == null)
				return;

			lock (typeof (Harvester)) {
				if (ms_oBackdoorData == null)
					ms_oBackdoorData = new SortedDictionary<int, Hopper>();

				ms_oBackdoorData[nCustomerMarketplaceID] = oHopper;
			} // lock
		} // SetBackdoorData

		private static SortedDictionary<int, Hopper> ms_oBackdoorData;

		private static Hopper FetchBackdoorData(int nCustomerMarketplaceID, ASafeLog log) {
			log.Debug("Harvester: fetching back-door data for marketplace {0}...", nCustomerMarketplaceID);

			Hopper oBackdoorData;

			lock (typeof(Harvester)) {
				if ((ms_oBackdoorData == null) || !ms_oBackdoorData.ContainsKey(nCustomerMarketplaceID)) {
					log.Debug("Harvester: no back-door data found for marketplace {0}.", nCustomerMarketplaceID);
					log.Debug("Harvester: fetching back-door data for marketplace {0} complete.", nCustomerMarketplaceID);
					return null;
				} // if

				oBackdoorData = ms_oBackdoorData[nCustomerMarketplaceID];

				ms_oBackdoorData.Remove(nCustomerMarketplaceID);
			} // lock

			log.Debug("Harvester: fetching back-door data for marketplace {0} complete.", nCustomerMarketplaceID);
			return oBackdoorData;
		} // FetchBackdoorData

		/// <summary>
		/// Constructs the Harvester object. The object is not ready to harvest. Call to Init() first.
		/// </summary>
		public Harvester(AccountData oAccountData, ASafeLog log) : base(log) {
			ErrorsToEmail = new SortedDictionary<string, string>();

			AccountData = oAccountData;
			VerboseLogging = false;
			Hopper = new Hopper(VatReturnSourceType.Linked);
			IsLoggedIn = false;
		} // constructor

		public int SourceID {
			get { return (int)Hopper.Source;}
		} // SourceID

		/// <summary>
		/// Output storage.
		/// </summary>
		public virtual Hopper Hopper { get; private set; } // Hopper

		/// <summary>
		/// Initializes the Harvester.
		/// </summary>
		/// <returns>true, if initialization was successful; false, otherwise.</returns>
		public virtual bool Init() {
			Session = new HttpClient {
				BaseAddress = new Uri("https://online.hmrc.gov.uk")
			};

			return true;
		} // Init

		public virtual void Run(bool bValidateCredentialsOnly) {
			Run(bValidateCredentialsOnly, 0);
		} // Run

		/// <summary>
		/// Main harvest function. Logs in to hmrc.gov.uk and fetches data.
		/// </summary>
		/// <param name="bValidateCredentialsOnly">true to validate credentials only,
		/// false to login and download data.</param>
		/// <param name="nCustomerMarketplaceID">Customer marketplace id for fetching back-door data.</param>
		public virtual void Run(bool bValidateCredentialsOnly, int nCustomerMarketplaceID) {
			Debug(
				"Harvester run mode: {0}.",
				bValidateCredentialsOnly ? "validate credentials only" : "login and download data"
			);

			try {
				if (!bValidateCredentialsOnly) {
					Hopper oBackdoorData = FetchBackdoorData(nCustomerMarketplaceID, this);

					if (oBackdoorData != null) {
						Hopper.FetchBackdoorData(oBackdoorData);
						Debug("Harvester running is complete.");
						return;
					} // if

					if (Password == VendorInfo.TopSecret) {
						if (0 < ObjectFactory.GetInstance<IUsersRepository>().ExternalUserCount(UserName)) {
							Debug(
								"This HMRC account for customer {0} was created from uploaded files, nothing to retrieve.",
								UserName
							);
							return;
						} // if HMRC login is customer's email
					} // if the password is...
				} // if do retrieve data
			} catch (Exception e) {
				throw new ApiException(e.Message, e);
			} // try

			try {
				if (!IsLoggedIn)
					Login(GetLoginRequestDetails(GetPage("")));
			} catch (InvalidCredentialsException) {
				throw;
			} catch (Exception e) {
				throw new ApiException(e.Message, e);
			} // try

			if (!IsLoggedIn)
				throw new ApiException("Not logged in.");

			if (bValidateCredentialsOnly) {
				Debug("Harvester running is complete.");
				return;
			} // if

			try {
				string sUserVatID = GetUserVatID();

				Debug("Harvester has validated login credentials.");

				Debug("Harvester starts downloading data.");

				VatReturns(sUserVatID);
				FetchRtiTaxYears();

				Debug("Harvester running is complete.");
			} catch (Exception e) {
				Alert(e, "Exception caught during retrieving HMRC data.");
				throw new ApiException(e.Message, e);
			} // try
		} // Run

		/// <summary>
		/// Performs cleanup.
		/// </summary>
		public virtual void Done() {
			Session.Dispose();
		} // Done

		public SortedDictionary<string, string> ErrorsToEmail { get; private set; }

		private AccountData AccountData { get; set; }

		/// <summary>
		/// Log verbose logging on (true) or off (false).
		/// </summary>
		private bool VerboseLogging { get; set; }

		/// <summary>
		/// User name for hmrc.gov.uk
		/// </summary>
		private string UserName {
			get { return (((AccountData == null) ? null : AccountData.Login) ?? "").Trim(); }
		} // UserName

		/// <summary>
		/// Password for hmrc.gov.uk
		/// </summary>
		private string Password {
			get { return (((AccountData == null) ? null : AccountData.Password) ?? "").Trim(); }
		} // Password

		private bool IsLoggedIn { get; set; }

		/// <summary>
		/// Stores login request details (method, URL, form field names).
		/// </summary>
		private struct LoginRequestDetails {
			/// <summary>
			/// Login method (GET, POST, etc).
			/// </summary>
			public string Method;

			/// <summary>
			/// Login page URL.
			/// </summary>
			public string Url;

			/// <summary>
			/// Form field name for user name.
			/// </summary>
			public string UserNameField;

			/// <summary>
			/// Form field name for password.
			/// </summary>
			public string PasswordField;

			/// <summary>
			/// Validates all the details. Quietly completes on success. Throws HarvesterException on error.
			/// </summary>
			public void Validate() {
				if (string.IsNullOrWhiteSpace(this.Method))
					throw new HarvesterException("Login method is empty.");

				if (string.IsNullOrWhiteSpace(this.Url))
					throw new HarvesterException("Login URL is empty.");

				if (string.IsNullOrWhiteSpace(this.UserNameField))
					throw new HarvesterException("Login user name field is empty.");

				if (string.IsNullOrWhiteSpace(this.PasswordField))
					throw new HarvesterException("Login password is empty.");
			} // Validate

			/// <summary>
			/// Converts this structure to string.
			/// </summary>
			/// <returns>String representation of the structure (mainly for logging).</returns>
			public override string ToString() {
				return string.Format(
					"Login method: {0} {1}\nUser name field: {2}\nPassword field: {3}",
					this.Method,
					this.Url,
					this.UserNameField,
					this.PasswordField
				);
			} // ToString
		} // LoginRequestDetails

		private string GetUserVatID() {
			Info("Retrieving user VAT id...");

			HtmlDocument doc = GetPage("/home/services");

			string sID;
			string oldWayError;
			string newWayError = null;

			GetUserVatIDOldFashionWay(doc, out sID, out oldWayError);

			if (sID == string.Empty)
				GetUserVatIDNewFashionWay(doc, out sID, out newWayError);

			if (sID == string.Empty)
				throw new HarvesterException((oldWayError + " " + newWayError).Trim());

			Info("User VAT id is '{0}'.", sID);

			ExtractTaxOfficeNumber(doc);

			return sID;
		} // GetUserVatID

		private void GetUserVatIDOldFashionWay(HtmlDocument doc, out string sID, out string errorMsg) {
			sID = string.Empty;
			errorMsg = string.Empty;

			HtmlNode oLink =
				doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVAT\"]")
				??
				doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVATMakeVATReturn\"]");

			if (oLink == null) {
				errorMsg = "Access VAT services link not found (old fashion way).";
				Debug(errorMsg);
				return;
			} // if

			const string href = "href";

			if (!oLink.Attributes.Contains(href)) {
				errorMsg = "Access VAT services link has no HREF attribute (old fashion way).";
				Debug(errorMsg);
				return;
			} // if

			string sHref = oLink.Attributes[href].Value;

			if (!sHref.StartsWith("/vat/trader/")) {
				errorMsg = "Failed to parse Access VAT services link (old fashion way).";
				Debug(errorMsg);
				return;
			} // if

			sID = sHref.Substring(sHref.LastIndexOf('/') + 1).Trim();

			if (string.IsNullOrEmpty(sID)) {
				errorMsg = "Could not extract user VAT id in an old fashion way.";
				Debug(errorMsg);
			} // if
		} // GetUserVatIDOldFashionWay

		private void GetUserVatIDNewFashionWay(HtmlDocument doc, out string sID, out string errorMsg) {
			sID = string.Empty;
			errorMsg = string.Empty;

			HtmlNode section = doc.DocumentNode.SelectSingleNode("//section[@id=\"section-vat\"]");

			if (section == null) {
				errorMsg = "VAT section not found (new fashion way).";
				Debug(errorMsg);
				return;
			} // if

			if (!section.HasChildNodes) {
				errorMsg = "VAT section is empty (new fashion way).";
				Debug(errorMsg);
				return;
			} // if

			HtmlNode para = section.Element("p");

			if (para == null) {
				errorMsg = "VAT id location not found (new fashion way).";
				Debug(errorMsg);
				return;
			} // if

			string innerText = (para.InnerText ?? string.Empty);

			if (!innerText.StartsWith("VAT registration number")) {
				errorMsg = "Failed to parse VAT id location (new fashion way).";
				Debug(errorMsg);
				return;
			} // if

			sID = innerText.Substring(innerText.IndexOf(':') + 1).Trim();

			if (string.IsNullOrEmpty(sID)) {
				errorMsg = "Could not extract user VAT id in a new fashion way.";
				Debug(errorMsg);
			} // if
		} // GetUserVatIDNewFashionWay

		/// <summary>
		/// Logs in to the Field.
		/// </summary>
		/// <param name="lrd">Login form details.</param>
		private void Login(LoginRequestDetails lrd) {
			try {
				if ((UserName == "") || (Password == ""))
					throw new ClientHarvesterException("Unspecified user name or password.");

				string sPassword = Password;
				const int nMaxPasswordLength = 12;

				if (sPassword.Length > nMaxPasswordLength) {
					Warn(
						"Supplied password ({0}) is too long, truncating to {1} characters.",
						new Encrypted(Password),
						nMaxPasswordLength
					);
					sPassword = sPassword.Substring(0, nMaxPasswordLength);
				} // if

				Info("Login URL: {0}", lrd.Url);
				Info("Logging in as {0}:{1}...", UserName, new Encrypted(sPassword));

				if (lrd.Method.ToUpper() != "POST")
					throw new HarvesterException("Unsupported login method: " + lrd.Method);

				var oData = new Dictionary<string, string>();
				oData[lrd.UserNameField] = UserName;
				oData[lrd.PasswordField] = sPassword;

				var fuec = new FormUrlEncodedContent(oData);

				foreach (KeyValuePair<string, IEnumerable<string>> h in fuec.Headers)
					foreach (string sHeaderValue in h.Value)
						Debug("Header - {0}: {1}", h.Key, sHeaderValue);

				HttpResponseMessage response = Session.PostAsync(lrd.Url, fuec).Result;

				Debug("Validating response code for user name {0}...", UserName);

				response.EnsureSuccessStatusCode();

				Info("Response code for user name {0} validated.", UserName);

				string sResponse = response.Content.ReadAsStringAsync().Result;

				var doc = new HtmlDocument();
				doc.LoadHtml(sResponse);

				HtmlNode oTitle = doc.DocumentNode.SelectSingleNode("/html/head/title");

				if (oTitle == null) {
					Error("No title in login response:");
					Debug("{0}", sResponse);
					throw new HarvesterException("No title in login response.");
				} // if

				switch (oTitle.InnerText.Trim()) {
				case "HMRC: Security message":
					IsLoggedIn = true;
					Info("User {0} logged in successfully.", UserName);
					break;

				case "HMRC: Error":
					IsLoggedIn = false;
					Warn("Not logged in: HMRC site error.");
					Debug("{0}", sResponse);
					throw new ClientHarvesterException("HMRC system is unavailable, please try again later.");

				case "HMRC: Login":
					IsLoggedIn = false;
					Warn("Not logged in: invalid user name or password.");
					Debug("{0}", sResponse);
					throw new InvalidCredentialsException("Invalid user name or password.");

				default:
					IsLoggedIn = false;
					Error("Not logged in because of unexpected page title in login response: {0}", oTitle.InnerText);
					Debug("{0}", sResponse);
					throw new HarvesterException("Unexpected title in login response.");
				} // switch
			} catch (InvalidCredentialsException) {
				throw;
			} catch (ClientHarvesterException) {
				throw;
			} catch (HarvesterException) {
				throw;
			} catch (Exception e) {
				throw new HarvesterException("Failed to log in: " + e.Message, e);
			} // try
		} // Login

		/// <summary>
		/// Extracts login form details from the login page.
		/// </summary>
		/// <param name="oLoginPage">Login page.</param>
		/// <returns>Login form details.</returns>
		private LoginRequestDetails GetLoginRequestDetails(HtmlDocument oLoginPage) {
			HtmlNodeCollection oForms = oLoginPage.DocumentNode.SelectNodes("//form[contains(@action,'login')]");

			if ((oForms == null) || (oForms.Count != 1))
				throw new HarvesterException("Login form not found or too many forms on the page.");

			HtmlNode oLoginForm = oForms[0];

			string sLoginUrl = oLoginForm.Attributes["action"].Value;

			string sLoginMethod = oLoginForm.Attributes.Contains("method") ? oLoginForm.Attributes["method"].Value : "GET";

			HtmlNode oUserName = oLoginForm.SelectSingleNode("//input[@id=\"FieldUserID\"]");

			if (oUserName == null)
				throw new HarvesterException("User name field not found.");

			if (!oUserName.Attributes.Contains("name"))
				throw new HarvesterException("User name field's NAME attribute not specified.");

			HtmlNode oPassword = oLoginForm.SelectSingleNode("//input[@id=\"FieldPassword\"]");

			if (oPassword == null)
				throw new HarvesterException("Password field not found.");

			if (!oPassword.Attributes.Contains("name"))
				throw new HarvesterException("Password field's NAME attribute not specified.");

			var oOutput = new LoginRequestDetails {
				Method = sLoginMethod,
				Url = sLoginUrl,
				UserNameField = oUserName.Attributes["name"].Value,
				PasswordField = oPassword.Attributes["name"].Value
			};

			Info(oOutput);

			oOutput.Validate();

			return oOutput;
		} // GetLoginRequestDetails

		/// <summary>
		/// Fetches a page from the Field (https://online.hmrc.gov.uk).
		/// Throws exception on error.
		/// </summary>
		/// <param name="sResource">Page address (without protocol and domain/port but with the leading slash).</param>
		/// <returns>Fetched page.</returns>
		private HtmlDocument GetPage(string sResource) {
			Info("Requesting {0}{1}", Session.BaseAddress.AbsoluteUri, sResource);

			HttpResponseMessage response = Session.GetAsync(sResource).Result;

			Info("Response received with code {0}", response.StatusCode);

			response.EnsureSuccessStatusCode();

			Info("Fetching the output...");

			string sPage = response.Content.ReadAsStringAsync().Result;

			if (VerboseLogging)
				Debug("--- Server response:\n\n{0}\n\n --- End of server response", sPage);

			Info("Page loaded, parsing...");

			var doc = new HtmlDocument();

			doc.LoadHtml(sPage);

			Info("Page {0}{1} is ready", Session.BaseAddress.AbsoluteUri, sResource);

			return doc;
		} // GetPage

		/// <summary>
		/// HTTP session.
		/// </summary>
		private HttpClient Session { get; set; }

		private SortedDictionary<string, SheafMetaData> m_oVatReturnsMetaData;
		private object vatReturnsMetaDataLock = new object();

		/// <summary>
		/// Fetches "VAT return" data and stores it in the Hopper.
		/// Separate files are fetched in parallel.
		/// </summary>
		private void VatReturns(string sUserVatID) {
			Dictionary<string, string> oSubmittedReturns = LoadSubmittedReturnsList(sUserVatID);

			lock (this.vatReturnsMetaDataLock) {
				this.m_oVatReturnsMetaData = new SortedDictionary<string, SheafMetaData>();
			} // lock

			if (oSubmittedReturns != null) {
				var oTasks = new List<Task>();

				foreach (KeyValuePair<string, string> sr in oSubmittedReturns) {
					string sHtmlUrl = string.Format(
						"/vat-file/trader/{0}{1}{2}",
						sUserVatID, 
						sUserVatID.EndsWith("/") || sr.Key.StartsWith("/") ? "" : "/",
						sr.Key
					);

					string sPdfUrl = sHtmlUrl + "?format=pdf";

					string sBaseFileName = sr.Value.Replace(' ', '_');

					Info("VatReturns: requesting {0}.html <- {1}", sBaseFileName, sHtmlUrl);

					lock (this.vatReturnsMetaDataLock) {
						this.m_oVatReturnsMetaData[sHtmlUrl] = new SheafMetaData {
							DataType = DataType.VatReturn,
							FileType = FileType.Html,
							BaseFileName = sBaseFileName,
							Thrasher = new VatReturnThrasher(VerboseLogging, this)
						};
					} // lock

					oTasks.Add(Session.GetAsync(sHtmlUrl).ContinueWith(GetFile));

					Info("VatReturns: requesting {0}.pdf <- {1}", sBaseFileName, sPdfUrl);

					lock (this.vatReturnsMetaDataLock) {
						this.m_oVatReturnsMetaData[sPdfUrl] = new SheafMetaData {
							DataType = DataType.VatReturn,
							FileType = FileType.Pdf,
							BaseFileName = sBaseFileName,
							Thrasher = null
						};
					} // lock

					oTasks.Add(Session.GetAsync(sPdfUrl).ContinueWith(GetFile));
				} // for each file

				Task.WaitAll(oTasks.ToArray());
			} // if
		} // VatReturns

		private Dictionary<string, string> LoadSubmittedReturnsList(string sUserVatID) {
			Info("Loading list of submitted VAT returns...");

			HtmlDocument doc = GetPage("/vat-file/trader/" + sUserVatID + "/periods");

			bool hasNoPrevious = doc.DocumentNode.InnerText.IndexOf(
				"There are no returns previously submitted available to view.",
				StringComparison.InvariantCulture
			) >= 0;

			if (hasNoPrevious) {
				Info("There are no returns previously submitted available to view.");
				return null;
			} // if

			HtmlNode oListTable = doc.DocumentNode.SelectSingleNode("//*[@id=\"VAT0011\"]/div[2]/table/tbody");

			if (oListTable == null)
				throw new HarvesterException("Failed to find list of returns.");

			if (oListTable.ChildNodes == null) {
				Info("Loading list of submitted VAT returns complete, no files found.");
				return null;
			} // if

			var oRes = new Dictionary<string, string>();

			foreach (HtmlNode oTR in oListTable.ChildNodes) {
				if (oTR.Name.ToUpper() != "TR")
					continue;

				if (oTR.ChildNodes == null)
					continue;

				HtmlNode oTD = oTR.SelectSingleNode("td");

				if (oTD == null)
					continue;

				HtmlNode oLink = oTD.SelectSingleNode("a");

				if (oLink == null)
					continue;

				if (!oLink.Attributes.Contains("href"))
					continue;

				string sHref = oLink.Attributes["href"].Value;

				if (string.IsNullOrWhiteSpace(sHref))
					continue;

				oRes[sHref] = oLink.InnerText;
			} // for each row

			Info(
				"Loading list of submitted VAT returns complete, {0} file{1} found.",
				oRes.Count,
				oRes.Count == 1 ? "" : "s"
			);

			return oRes;
		} // Load

		/// <summary>
		/// Fetches one file from the Field and stores it in the Hopper as byte[].
		/// Adds HarvesterError to the Hopper in case of error.
		/// </summary>
		/// <param name="task">HTTP request result.</param>
		private void GetFile(Task<HttpResponseMessage> task) {
			SheafMetaData fi;

			HttpResponseMessage response = task.Result;

			var sUrl = response.RequestMessage.RequestUri.PathAndQuery;

			lock (this.vatReturnsMetaDataLock) {
				fi = this.m_oVatReturnsMetaData[sUrl];
			} // lock

			Info("GetFile: retrieving {0}", response.RequestMessage);

			if (!response.IsSuccessStatusCode) {
				string sErrMsg = response.StatusCode.ToString() + ": " + response.ReasonPhrase;

				Error("Not saving because of error. Status code {0}", sErrMsg);
				Hopper.Add(fi, response);

				lock (ErrorsToEmail) {
					ErrorsToEmail[sUrl] = sErrMsg;
				} // lock

				return;
			} // if

			Stream oInputStream = response.Content.ReadAsStreamAsync().Result;

			var oOutput = new MemoryStream();

			var outputFile = new BinaryWriter(oOutput);

			const int nBufSize = 8192;
			var buf = new byte[nBufSize];

			int nRead = oInputStream.Read(buf, 0, nBufSize);

			while (nRead > 0) {
				outputFile.Write(buf, 0, nRead);

				nRead = oInputStream.Read(buf, 0, nBufSize);
			} // while

			outputFile.Close();

			byte[] oFile = oOutput.ToArray();

			Hopper.Add(fi, oFile);

			if (fi.Thrasher != null) {
				ISeeds x = fi.Thrasher.Run(fi, oFile);

				if (x == null) {
					var vrs = (VatReturnSeeds)fi.Thrasher.Seeds;

					lock (ErrorsToEmail) {
						ErrorsToEmail[sUrl] = vrs.FatalError;
					} // lock
				} // if

				Hopper.Add(fi, x);
			} // if
		} // GetFile

		private string TaxOfficeNumber { get; set; }

		private void ExtractTaxOfficeNumber(HtmlDocument doc) {
			const string sBaseXPath = "//dl[contains(@class, 'known-facts')]";

			HtmlNodeCollection oDataLists = doc.DocumentNode.SelectNodes(sBaseXPath);

			if (oDataLists == null) {
				Warn("No suitable location for Tax Office Number found.");
				return;
			} // if

			foreach (HtmlNode oDL in oDataLists) {
				HtmlNode oDT = oDL.SelectSingleNode("./dt");

				if (oDT == null)
					continue;

				if (oDT.InnerText != "Tax Office Number:")
					continue;

				HtmlNode oDD = oDL.SelectSingleNode("./dd");

				if (oDD == null)
					throw new HarvesterException("Tax Office Number not found.");

				TaxOfficeNumber = oDD.InnerText.Trim().Replace(" ", "");

				if (TaxOfficeNumber == string.Empty)
					throw new HarvesterException("Tax Office Number not specified.");

				Info("Tax office number is {0}.", TaxOfficeNumber);
				return;
			} // for each data list

			Warn("Tax Office Number location not found.");
		} // ExtractTaxOfficeNumber

		private void FetchRtiTaxYears() {
			if (string.IsNullOrWhiteSpace(TaxOfficeNumber)) {
				Debug("Not fetching RTI Tax Years: Tax Office number is empty.");
				return;
			} // if

			Debug("Fetching RTI Tax Years started...");

			HtmlDocument doc = GetPage("/paye/org/" + TaxOfficeNumber + "/account");

			if ((doc == null) || (doc.DocumentNode == null))
				throw new HarvesterException("Failed to fetch PAYE account page.");

			var oOutput = new MemoryStream();

			doc.Save(oOutput);

			var smd = new SheafMetaData {
				BaseFileName = "PAYE RTI Tax Year",
				DataType = DataType.PayeRtiTaxYears,
				FileType = FileType.Html,
				Thrasher = null
			};

			Hopper.Add(smd, oOutput.ToArray());

			HtmlNode oTHead = doc.DocumentNode.SelectSingleNode("//*[@id=\"top\"]/div[3]/div[2]/div/div[2]/table[1]/thead");

			if (oTHead == null) {
				Info("RTI tax years table head not found.");
				return;
			} // if

			HtmlNodeCollection oHeadRows = oTHead.SelectNodes("tr");

			if ((oHeadRows == null) || (oHeadRows.Count != 1))
				throw new HarvesterException("RTI tax years table head is empty.");

			HtmlNodeCollection oHeadCells = oHeadRows[0].SelectNodes("th | td");

			string[] aryExpectedColumnHeaders = new [] {
				"Date",
				"Amount paid in period",
				"Amount due in period",
			};

			if ((oHeadCells == null) || (oHeadCells.Count != aryExpectedColumnHeaders.Length))
				throw new HarvesterException(string.Format("Failed to fetch RTI tax years: no cells in header row"));

			for (int i = 0; i < aryExpectedColumnHeaders.Length; i++) {
				if (!oHeadCells[i].InnerText.Trim().StartsWith(aryExpectedColumnHeaders[i])) {
					Info(
						"Not fetching RTI tax years: unexpected column {0} name: {1} (expected: {2})",
						i, oHeadCells[i].InnerText, aryExpectedColumnHeaders[i]
					);
					return;
				} // if
			} // for

			HtmlNode oTBody = doc.DocumentNode.SelectSingleNode("//*[@id=\"top\"]/div[3]/div[2]/div/div[2]/table[1]/tbody");

			if (oTBody == null)
				throw new HarvesterException("RTI tax years table body not found.");

			HtmlNodeCollection oRows = oTBody.SelectNodes("tr");

			if ((oRows == null) || (oRows.Count < 1))
				throw new HarvesterException("RTI tax years data not found.");

			bool bFirst = true;
			int nRowNum = -1;

			int nFirstYear = 0;
			int nLastYear = 0;

			var data = new List<RtiTaxYearRowData>();

			foreach (HtmlNode oTR in oRows) {
				nRowNum++;

				HtmlNodeCollection oCells = oTR.SelectNodes("th | td");

				if ((oCells == null) || (oCells.Count < 1)) {
					throw new HarvesterException(string.Format(
						"Failed to fetch RTI tax years: no cells in row {0}.",
						nRowNum
					));
				} // if

				if (bFirst) {
					bFirst = false;

					HtmlNode oCell = oCells[0];

					if (!oCell.Attributes.Contains("colspan") || (oCell.Attributes["colspan"].Value != "3")) {
						throw new HarvesterException(string.Format(
							"Failed to fetch RTI tax years: incorrect format in row {0}",
							nRowNum
						));
					} // if

					if (oCell.InnerText.Trim() == "Previous tax years")
						break;

					MatchCollection match = Regex.Matches(oCell.InnerText.Trim(), @"^Current tax year (\d\d)(\d\d)-(\d\d)$");

					if (match.Count != 1) {
						throw new HarvesterException(string.Format(
							"Failed to fetch RTI tax years: incorrect content in row {0}.",
							nRowNum
						));
					} // if

					GroupCollection grp = match[0].Groups;
					if (grp.Count != 4) {
						throw new HarvesterException(string.Format(
							"Failed to fetch RTI tax years: unexpected content in row {0}.",
							nRowNum
						));
					} // if

					nFirstYear = Convert.ToInt32(grp[1].Value) * 100 + Convert.ToInt32(grp[2].Value);
					nLastYear = Convert.ToInt32(grp[1].Value) * 100 + Convert.ToInt32(grp[3].Value);

					Info("Current tax year: {0} - {1}", nFirstYear, nLastYear);

					continue;
				} // if first row

				string sFirstCell = oCells.Count > 0 ? oCells[0].InnerText.Trim() : string.Empty;

				if (oCells.Count != 3) {
					if ((oCells.Count == 1) && (sFirstCell == "Previous tax years"))
						break;

					throw new HarvesterException(string.Format(
						"Failed to fetch RTI tax years: unexpected number of cells in row {0}.",
						nRowNum
					));
				} // if

				if (sFirstCell == "Total")
					break;

				try {
					data.Add(new RtiTaxYearRowData(sFirstCell, oCells[1].InnerText.Trim(), oCells[2].InnerText.Trim()));
				} catch (Exception e) {
					throw new HarvesterException(
						string.Format(
							"Failed to fetch RTI tax years: unexpected format in row {0}.",
							nRowNum
						),
						e
					);
				} // try
			} // for each row

			int nCurYear = nFirstYear;

			var rtys = new RtiTaxYearSeeds();

			foreach (RtiTaxYearRowData rd in data.ToArray().Reverse()) {
				rtys.Months.Add(new RtiTaxMonthSeed {
					DateStart = new DateTime(nCurYear, rd.MonthStart, rd.DayStart),
					DateEnd = new DateTime(nCurYear, rd.MonthEnd, rd.DayEnd),
					AmountPaid = new Coin(rd.AmountPaid, "GBP"),
					AmountDue = new Coin(rd.AmountDue, "GBP")
				});

				if (rd.MonthStart == 12)
					nCurYear = nLastYear;
			} // for each

			Hopper.Add(smd, rtys);

			Debug("Fetching RTI Tax Years complete.");
		} // FetchRtiTaxYears
	} // class Harvester
} // namespace Ezbob.HmrcHarvester
