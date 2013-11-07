﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EzBob.CommonLib.Security;
using Ezbob.Logger;
using HtmlAgilityPack;
using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;
using log4net;
using DBCustomer = EZBob.DatabaseLib.Model.Database.Customer;

namespace Ezbob.HmrcHarvester {
	#region class Harvester

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
		#region public

		#region static

		#region backdoor data

		#region method SetBackdoorData

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

		#endregion method SetBackdoorData

		#region method FetchBackdoorData

		private static Hopper FetchBackdoorData(int nCustomerMarketplaceID, ASafeLog log) {
			log.Debug("Harvester: fetching backdoor data for marketplace {0}...", nCustomerMarketplaceID);

			Hopper oBackdoorData = null;

			lock (typeof(Harvester)) {
				if ((ms_oBackdoorData == null) || !ms_oBackdoorData.ContainsKey(nCustomerMarketplaceID)) {
					log.Debug("Harvester: no backdoor data found for marketplace {0}.", nCustomerMarketplaceID);
					log.Debug("Harvester: fetching backdoor data for marketplace {0} complete.", nCustomerMarketplaceID);
					return null;
				} // if

				oBackdoorData = ms_oBackdoorData[nCustomerMarketplaceID];

				ms_oBackdoorData.Remove(nCustomerMarketplaceID);
			} // lock

			log.Debug("Harvester: fetching backdoor data for marketplace {0} complete.", nCustomerMarketplaceID);
			return oBackdoorData;
		} // FetchBackdoorData

		#endregion method FetchBackdoorData

		#endregion backdoor data

		#region environment notification flag

		#region method SetRunningInWebEnvFlag

		public static void SetRunningInWebEnvFlag(int nCustomerMarketplaceID) {
			lock (typeof (Harvester)) {
				if (ms_oWebEnvFlag == null)
					ms_oWebEnvFlag = new SortedSet<int>();

				ms_oWebEnvFlag.Add(nCustomerMarketplaceID);
			}// lock
		} // SetRunningInWebEnvFlag

		private static SortedSet<int> ms_oWebEnvFlag; 

		#endregion method SetRunningInWebEnvFlag

		#region method FetchRunningInWebEnvFlag

		public static bool FetchRunningInWebEnvFlag(int nCustomerMarketplaceID, ASafeLog log) {
			log.Debug("Harvester: fetching running in web env flag for marketplace {0}...", nCustomerMarketplaceID);

			lock (typeof(Harvester)) {
				if ((ms_oWebEnvFlag == null) || !ms_oWebEnvFlag.Contains(nCustomerMarketplaceID)) {
					log.Debug("Harvester: running in web env flag is false for marketplace {0}.", nCustomerMarketplaceID);
					log.Debug("Harvester: fetching running in web env flag for marketplace {0} complete.", nCustomerMarketplaceID);
					return false;
				} // if

				ms_oWebEnvFlag.Remove(nCustomerMarketplaceID);
			} // lock

			log.Debug("Harvester: running in web env flag is true for marketplace {0}.", nCustomerMarketplaceID);
			log.Debug("Harvester: fetching running in web env flag for marketplace {0} complete.", nCustomerMarketplaceID);
			return true;
		} // FetchRunningInWebEnvFlag

		#endregion method FetchRunningInWebEnvFlag

		#endregion environment notification flag

		#endregion static

		#region constructor

		/// <summary>
		/// Constructs the Harvester object. The object is not ready to harvest. Call to Init() first.
		/// </summary>
		public Harvester(AccountData oAccountData, ILog log) : base(log) {
			AccountData = oAccountData;
			VerboseLogging = false;
			Hopper = new Hopper();
			IsLoggedIn = false;
		} // constructor

		#endregion constructor

		#region property Hopper

		/// <summary>
		/// Output storage.
		/// </summary>
		public virtual Hopper Hopper { get; private set; } // Hopper

		#endregion property Hopper

		#region method Init

		/// <summary>
		/// Initialises the Harvester.
		/// </summary>
		/// <returns>true, if initialisation was successful; false, otherwise.</returns>
		public virtual bool Init() {
			Session = new HttpClient {
				BaseAddress = new Uri("https://online.hmrc.gov.uk")
			};

			return true;
		} // Init

		#endregion method Init

		#region method Run

		public virtual void Run(bool bValidateCredentialsOnly) {
			Run(bValidateCredentialsOnly, 0);
		} // Run

		/// <summary>
		/// Main harvest function. Logs in to hmrc.gov.uk and fetches data.
		/// </summary>
		/// <param name="bValidateCredentialsOnly">true to validate credentials only, false to login and download data.</param>
		/// <param name="nCustomerMarketplaceID">Customer marketplace id for fetching backdoor data.</param>
		public virtual void Run(bool bValidateCredentialsOnly, int nCustomerMarketplaceID) {
			Debug("Harvester run mode: {0}.", bValidateCredentialsOnly ? "validate credentials only" : "login and download data");

			try {
				if (!bValidateCredentialsOnly) {
					Hopper oBackdoorData = FetchBackdoorData(nCustomerMarketplaceID, this);

					if (oBackdoorData != null) {
						Hopper.FetchBackdoorData(oBackdoorData);
						Debug("Harvester running is complete.");
						return;
					} // if
				} // if
			}
			catch (Exception e) {
				throw new ApiException(e.Message, e);
			} // try

			try {
				if (!IsLoggedIn)
					Login(GetLoginRequestDetails(GetPage("")));
			}
			catch (Exception e) {
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
			}
			catch (Exception e) {
				throw new ApiException(e.Message, e);
			} // try
		} // Run

		#endregion method Run

		#region method Done

		/// <summary>
		/// Performs cleanup.
		/// </summary>
		public virtual void Done() {
			Session.Dispose();
		} // Done

		#endregion method Done

		#endregion public

		#region private

		#region infrastructure

		#region property AccountData

		private AccountData AccountData { get; set; }

		#endregion property AccountData

		#region property VerboseLogging

		/// <summary>
		/// Log verbose logging on (true) or off (false).
		/// </summary>
		private bool VerboseLogging { get; set; }

		#endregion property VerboseLogging

		#region property UserName

		/// <summary>
		/// User name for hmrc.gov.uk
		/// </summary>
		private string UserName {
			get { return (((AccountData == null) ? null : AccountData.Login) ?? "").Trim(); }
		} // UserName

		#endregion property UserName

		#region property Password

		/// <summary>
		/// Password for hmrc.gov.uk
		/// </summary>
		private string Password {
			get { return (((AccountData == null) ? null : AccountData.Password) ?? "").Trim(); }
		} // Password

		#endregion property Password

		#region property IsLoggedIn

		private bool IsLoggedIn { get; set; }

		#endregion property IsLoggedIn

		#region struct LoginRequestDetails

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
				if (string.IsNullOrWhiteSpace(Method))
					throw new HarvesterException("Login method is empty.");

				if (string.IsNullOrWhiteSpace(Url))
					throw new HarvesterException("Login URL is empty.");

				if (string.IsNullOrWhiteSpace(UserNameField))
					throw new HarvesterException("Login user name field is empty.");

				if (string.IsNullOrWhiteSpace(PasswordField))
					throw new HarvesterException("Login password is empty.");
			} // Validate

			/// <summary>
			/// Converts this structure to string.
			/// </summary>
			/// <returns>String representation of the structure (mainly for logging).</returns>
			public override string ToString() {
				return string.Format(
					"Login method: {0} {1}\nUser name field: {2}\nPassword field: {3}",
					Method, Url, UserNameField, PasswordField
				);
			} // ToString
		} // LoginRequestDetails

		#endregion struct LoginRequestDetails

		#region method GetUserVatID

		private string GetUserVatID() {
			Info("Retrieving user VAT id...");

			HtmlDocument doc = GetPage("/home/services");

			HtmlNode oLink =
				doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVAT\"]")
				??
				doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVATMakeVATReturn\"]");

			if (oLink == null)
				throw new HarvesterException("Access VAT services link not found.");

			if (!oLink.Attributes.Contains("href"))
				throw new HarvesterException("Access VAT services link has no HREF attrbute.");

			string sHref = oLink.Attributes["href"].Value;

			if (!sHref.StartsWith("/vat/trader/"))
				throw new HarvesterException("Failed to parse Access VAT services link.");

			string sID = sHref.Substring(sHref.LastIndexOf('/') + 1);

			Info("User VAT id is {0}.", sID);

			ExtractTaxOfficeNumber(doc);

			return sID;
		} // GetUserVatID

		#endregion method GetUserVatID

		#region method Login

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
					Warn("Supplied password ({0}) is too long, truncating to {1} characters.", Encryptor.Encrypt(Password), nMaxPasswordLength);
					sPassword = sPassword.Substring(0, nMaxPasswordLength);
				} // if

				Info("Login URL: {0}", lrd.Url);
				Info("Logging in as {0}:{1}...", UserName, Encryptor.Encrypt(sPassword));

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
					throw new ClientHarvesterException("Invalid user name or password.");

				default:
					IsLoggedIn = false;
					Error("Not logged in because of unexpeced page title in login response: {0}", oTitle.InnerText);
					Debug("{0}", sResponse);
					throw new HarvesterException("Unexpected title in login response.");
				} // switch
			}
			catch (ClientHarvesterException) {
				throw;
			}
			catch (Exception e) {
				throw new HarvesterException("Failed to log in: " + e.Message, e);
			} // try
		} // Login

		#endregion method Login

		#region method GetLoginRequestDetails

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

		#endregion method GetLoginRequestDetails

		#region method GetPage

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

		#endregion method GetPage

		#region property Session

		/// <summary>
		/// HTTP session.
		/// </summary>
		private HttpClient Session { get; set; }

		#endregion property Session

		#endregion infrastructure

		#region VAT Return

		#region field VatReturnsMetaData

		private SortedDictionary<string, SheafMetaData> m_oVatReturnsMetaData;

		#endregion field VatReturnsMetaData

		#region method VatReturns

		/// <summary>
		/// Fetches "VAT return" data and stores it in the Hopper.
		/// Separate files are fetched in parallel.
		/// </summary>
		private void VatReturns(string sUserVatID) {
			Dictionary<string, string> oSubmittedReturns = LoadSubmittedReturnsList(sUserVatID);

			m_oVatReturnsMetaData = new SortedDictionary<string, SheafMetaData>();

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

					m_oVatReturnsMetaData[sHtmlUrl] = new SheafMetaData {
						DataType = DataType.VatReturn,
						FileType = FileType.Html,
						BaseFileName = sBaseFileName,
						Thrasher = new VatReturnThrasher(VerboseLogging, this)
					};

					oTasks.Add(Session.GetAsync(sHtmlUrl).ContinueWith(GetFile));

					Info("VatReturns: requesting {0}.pdf <- {1}", sBaseFileName, sPdfUrl);

					m_oVatReturnsMetaData[sPdfUrl] = new SheafMetaData {
						DataType = DataType.VatReturn,
						FileType = FileType.Pdf,
						BaseFileName = sBaseFileName,
						Thrasher = null
					};

					oTasks.Add(Session.GetAsync(sPdfUrl).ContinueWith(GetFile));
				} // for each file

				Task.WaitAll(oTasks.ToArray());
			} // if
		} // VatReturns

		#endregion method VatReturns

		#region method LoadSubmittedReturnsList

		private Dictionary<string, string> LoadSubmittedReturnsList(string sUserVatID) {
			Info("Loading list of submitted VAT returns...");

			HtmlDocument doc = GetPage("/vat-file/trader/" + sUserVatID + "/periods");

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

			Info("Loading list of submitted VAT returns complete, {0} file{1} found", oRes.Count, oRes.Count == 1 ? "" : "s");

			return oRes;
		} // Load

		#endregion method LoadSubmittedReturnsList

		#region method GetFile

		/// <summary>
		/// Fetches one file from the Field and stores it in the Hopper as byte[].
		/// Adds HarvesterError to the Hopper in case of error.
		/// </summary>
		/// <param name="task">HTTP request result.</param>
		/// <param name="oFileIdentifier">Where to save the file in the Hopper.</param>
		private void GetFile(Task<HttpResponseMessage> task) {
			SheafMetaData fi = null;

			HttpResponseMessage response = task.Result;

			lock (m_oVatReturnsMetaData) {
				fi = m_oVatReturnsMetaData[response.RequestMessage.RequestUri.PathAndQuery];
			} // lock

			Info("GetFile: retrieving {0}", response.RequestMessage);

			if (!response.IsSuccessStatusCode) {
				Error("Not saving because of error. Status code {0}: {1}", response.StatusCode.ToString(), response.ReasonPhrase);
				Hopper.Add(fi, response);
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

			if (fi.Thrasher != null)
				Hopper.Add(fi, fi.Thrasher.Run(fi, oFile));
		} // GetFile

		#endregion method GetFile

		#endregion VAT Return

		#region PAYE

		#region property TaxOfficeNumber

		private string TaxOfficeNumber { get; set; }

		#endregion property TaxOfficeNumber

		#region method ExtractTaxOfficeNumber

		private void ExtractTaxOfficeNumber(HtmlDocument doc) {
			string sBaseXPath = "//dl[contains(@class, 'known-facts')]";

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

		#endregion method ExtractTaxOfficeNumber

		#region method FetchRtiTaxYears

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

			var aryExpectedColumnHeaders = new string[] {
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

				if ((oCells == null) || (oCells.Count < 1))
					throw new HarvesterException(string.Format("Failed to fetch RTI tax years: no cells in row {0}", nRowNum));

				if (bFirst) {
					HtmlNode oCell = oCells[0];

					if (!oCell.Attributes.Contains("colspan") || (oCell.Attributes["colspan"].Value != "3"))
						throw new HarvesterException(string.Format("Failed to fetch RTI tax years: incorrect format in row {0}", nRowNum));

					MatchCollection match = Regex.Matches(oCell.InnerText.Trim(), @"^Current tax year (\d\d)(\d\d)-(\d\d)$");

					if (match.Count != 1)
						throw new HarvesterException(string.Format("Failed to fetch RTI tax years: incorrect content in row {0}", nRowNum));

					GroupCollection grp = match[0].Groups;
					if (grp.Count != 4)
						throw new HarvesterException(string.Format("Failed to fetch RTI tax years: unexpected content in row {0}", nRowNum));

					nFirstYear = Convert.ToInt32(grp[1].Value) * 100 + Convert.ToInt32(grp[2].Value);
					nLastYear = Convert.ToInt32(grp[1].Value) * 100 + Convert.ToInt32(grp[3].Value);

					Info("Current tax year: {0} - {1}", nFirstYear, nLastYear);

					bFirst = false;
					continue;
				} // if first row

				if (oCells.Count != 3)
					throw new HarvesterException(string.Format("Failed to fetch RTI tag years: unexpected number of cells in row {0}", nRowNum));

				string sFirstCell = oCells[0].InnerText.Trim();

				if (sFirstCell == "Total")
					continue;

				try {
					data.Add(new RtiTaxYearRowData(sFirstCell, oCells[1].InnerText.Trim(), oCells[2].InnerText.Trim())); 
				}
				catch (Exception e) {
					throw new HarvesterException(string.Format("Failed to fetch RTI tax years: unexpected format in row {0}", nRowNum), e);
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

				if (rd.DayStart == 12)
					nCurYear = nLastYear;
			} // for each

			Hopper.Add(smd, rtys);

			Debug("Fetching RTI Tax Years complete.");
		} // FetchRtiTaxYears

		#endregion method FetchRtiTaxYears

		#endregion PAYE

		#endregion private
	} // class Harvester

	#endregion class Harvester
} // namespace Ezbob.HmrcHarvester
