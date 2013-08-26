using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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

		/// <summary>
		/// Main harvest function. Logs in to hmrc.gov.uk and fetches data.
		/// </summary>
		/// <param name="bValidateCredentialsOnly">true to validate credentials only, false to login and download data.</param>
		public virtual void Run(bool bValidateCredentialsOnly) {
			try {
				Debug("Harvester run mode: {0}.", bValidateCredentialsOnly ? "validate credentials only" : "login and download data");

				if (!IsLoggedIn)
					Login(GetLoginRequestDetails(GetPage("")));

				string sUserVatID = GetUserVatID();

				Debug("Harvester has validated login credentials.");

				IsLoggedIn = true;

				if (bValidateCredentialsOnly) {
					Debug("Harvester running is complete.");
					return;
				} // if

				Debug("Harvester starts downloading data.");

				VatReturns(sUserVatID);

				Debug("Harvester running is complete.");
			}
			catch (Exception e) {
				throw new ApiException("HMRC Harvester.Run failed", e);
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

		#region method GetUserVatID

		private string GetUserVatID() {
			Info("Retrieving user VAT id...");

			HtmlDocument doc = GetPage("/home/services");

			HtmlNode oLink = doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVAT\"]");

			if (oLink == null)
				throw new HarvesterException("Access VAT services link not found.");

			if (!oLink.Attributes.Contains("href"))
				throw new HarvesterException("Access VAT services link has not HREF attrbute.");

			string sHref = oLink.Attributes["href"].Value;

			if (!sHref.StartsWith("/vat/trader/"))
				throw new HarvesterException("Failed to parse Access VAT services link.");

			string sID = sHref.Substring(sHref.LastIndexOf('/') + 1);

			Info("User VAT id is {0}.", sID);

			return sID;
		} // GetUserVatID

		#endregion method GetUserVatID

		#region method Login

		/// <summary>
		/// Logs in to the Field.
		/// </summary>
		/// <param name="lrd">Login form details.</param>
		private void Login(LoginRequestDetails lrd) {
			if ((UserName == "") || (Password == ""))
				throw new HarvesterException("Unspecified user name or password.");

			Info("Logging in as {0}...", UserName);

			if (lrd.Method.ToUpper() != "POST")
				throw new HarvesterException("Unsupported login method: " + lrd.Method);

			var oData = new Dictionary<string, string>();
			oData[lrd.UserNameField] = UserName;
			oData[lrd.PasswordField] = Password;

			HttpResponseMessage response = Session.PostAsync(lrd.Url, new FormUrlEncodedContent(oData)).Result;

			Debug("Validating response code...");

			response.EnsureSuccessStatusCode();

			Info("Logged in as {0}.", UserName);
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

		#region field VatReturnsMetaData

		private SortedDictionary<string, SheafMetaData> m_oVatReturnsMetaData;

		#endregion field VatReturnsMetaData

		#endregion private
	} // class Harvester

	#endregion class Harvester
} // namespace Ezbob.HmrcHarvester
