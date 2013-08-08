using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ezbob.Logger;
using HtmlAgilityPack;

namespace Ezbob.HmrcCrawler {
	#region class Crawler

	public class Crawler : SafeLog {
		#region public

		#region constructor

		public Crawler(string sUserName, string sPassword, ASafeLog oLog = null)
			: base(oLog) {
			UserName = sUserName;
			Password = sPassword;
		} // constructor

		#endregion constructor

		#region property UserName

		public string UserName { get; set; }

		#endregion property UserName

		#region property Password

		public string Password { get; set; }

		#endregion property Password

		#region method Init

		public bool Init() {
			Session = new HttpClient {
				BaseAddress = new Uri("https://online.hmrc.gov.uk")
			};

			return true;
		} // Init

		#endregion method Init

		#region method Run

		public void Run() {
			LoginRequestDetails lrd = GetLoginRequestDetails(GetPage(""));

			Login(lrd);

			string sUserVatID = GetUserVatID();

			Dictionary<string, string> oSubmittedReturns = LoadSubmittedReturnsList(sUserVatID);

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

					Info("Downloading {0}.html <- {1}", sBaseFileName, sHtmlUrl);

					oTasks.Add(
						Session.GetAsync(sHtmlUrl).ContinueWith(SaveFile, sBaseFileName + ".html")
					);

					Info("Downloading {0}.pdf <- {1}", sBaseFileName, sPdfUrl);

					oTasks.Add(
						Session.GetAsync(sPdfUrl).ContinueWith(SaveFile, sBaseFileName + ".pdf")
					);
				} // for each file

				Task.WaitAll(oTasks.ToArray());
			} // if
		} // Run

		#endregion method Run

		#region method Done

		public void Done() {
			Session.Dispose();
		} // Done

		#endregion method Done

		#endregion public

		#region private

		#region method SaveFile

		private void SaveFile(Task<HttpResponseMessage> task, object oBaseFileName) {
			string sFileName = Path.Combine(Directory.GetCurrentDirectory(), "files", oBaseFileName.ToString());

			HttpResponseMessage response = task.Result;

			Info("Saving {0} <- {1}", sFileName, response.RequestMessage);

			if (!response.IsSuccessStatusCode) {
				Error("Not saving because of error. Status code {0}: {1}", response.StatusCode.ToString(), response.ReasonPhrase);
				return;
			} // if

			Stream oInputStream = response.Content.ReadAsStreamAsync().Result;

			var outputFile = new BinaryWriter(File.Open(sFileName, FileMode.Create));

			const int nBufSize = 8192;
			var buf = new byte[nBufSize];

			int nRead = oInputStream.Read(buf, 0, nBufSize);

			while (nRead > 0) {
				outputFile.Write(buf, 0, nRead);

				nRead = oInputStream.Read(buf, 0, nBufSize);
			} // while

			outputFile.Close();
		} // SaveFile

		#endregion method SaveFile

		#region method LoadSubmittedReturnsList

		private Dictionary<string, string> LoadSubmittedReturnsList(string sUserVatID) {
			Info("Loading list of submitted VAT returns...");

			HtmlDocument doc = GetPage("/vat-file/trader/" + sUserVatID + "/periods");

			HtmlNode oListTable = doc.DocumentNode.SelectSingleNode("//*[@id=\"VAT0011\"]/div[2]/table/tbody");

			if (oListTable == null)
				throw new CrawlerException("Failed to find list of returns.");

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
				throw new CrawlerException("Access VAT services link not found.");

			if (!oLink.Attributes.Contains("href"))
				throw new CrawlerException("Access VAT services link has not HREF attrbute.");

			string sHref = oLink.Attributes["href"].Value;

			if (!sHref.StartsWith("/vat/trader/"))
				throw new CrawlerException("Failed to parse Access VAT services link.");

			string sID = sHref.Substring(sHref.LastIndexOf('/') + 1);

			Info("User VAT id is {0}.", sID);

			return sID;
		} // GetUserVatID

		#endregion method GetUserVatID

		#region method Login

		private void Login(LoginRequestDetails lrd) {
			Info("Logging in as {0}...", UserName);

			if (lrd.Method.ToUpper() != "POST")
				throw new CrawlerException("Unsupported login method: " + lrd.Method);

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

		private LoginRequestDetails GetLoginRequestDetails(HtmlDocument oLoginPage) {
			HtmlNodeCollection oForms = oLoginPage.DocumentNode.SelectNodes("//form[contains(@action,'login')]");

			if ((oForms == null) || (oForms.Count != 1))
				throw new CrawlerException("Login form not found or too many forms on the page.");

			HtmlNode oLoginForm = oForms[0];

			string sLoginUrl = oLoginForm.Attributes["action"].Value;

			string sLoginMethod = oLoginForm.Attributes.Contains("method") ? oLoginForm.Attributes["method"].Value : "GET";

			HtmlNode oUserName = oLoginForm.SelectSingleNode("//input[@id=\"FieldUserID\"]");

			if (oUserName == null)
				throw new CrawlerException("User name field not found.");

			if (!oUserName.Attributes.Contains("name"))
				throw new CrawlerException("User name field's NAME attribute not specified.");

			HtmlNode oPassword = oLoginForm.SelectSingleNode("//input[@id=\"FieldPassword\"]");

			if (oPassword == null)
				throw new CrawlerException("Password field not found.");

			if (!oPassword.Attributes.Contains("name"))
				throw new CrawlerException("Password field's NAME attribute not specified.");

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

		private HtmlDocument GetPage(string sResource) {
			Info("Requesting {0}{1}", Session.BaseAddress.AbsoluteUri, sResource);

			HttpResponseMessage response = Session.GetAsync(sResource).Result;

			Info("Response received with code {0}", response.StatusCode);

			response.EnsureSuccessStatusCode();

			Info("Fetching the output...");

			string sPage = response.Content.ReadAsStringAsync().Result;

			Debug("--- Output:\n\n{0}\n\n --- End of output", sPage);

			Info("Page loaded, parsing...");

			var doc = new HtmlDocument();

			doc.LoadHtml(sPage);

			Info("Page {0}{1} is ready", Session.BaseAddress.AbsoluteUri, sResource);

			return doc;
		} // GetPage

		#endregion method GetPage

		#region property Session

		private HttpClient Session { get; set; }

		#endregion property Session

		#region struct LoginRequestDetails

		private struct LoginRequestDetails {
			public string Method;
			public string Url;
			public string UserNameField;
			public string PasswordField;

			public void Validate() {
				if (string.IsNullOrWhiteSpace(this.Method))
					throw new CrawlerException("Login method is empty.");

				if (string.IsNullOrWhiteSpace(this.Url))
					throw new CrawlerException("Login URL is empty.");

				if (string.IsNullOrWhiteSpace(this.UserNameField))
					throw new CrawlerException("Login user name field is empty.");

				if (string.IsNullOrWhiteSpace(this.PasswordField))
					throw new CrawlerException("Login password is empty.");
			} // Validate

			public override string ToString() {
				return string.Format(
					"Login method: {0} {1}\nUser name field: {2}\nPassword field: {3}",
					this.Method, this.Url, this.UserNameField, this.PasswordField
				);
			} // ToString
		} // LoginRequestDetails

		#endregion struct LoginRequestDetails

		#endregion private
	} // class Crawler

	#endregion class Crawler
} // namespace Ezbob.HmrcCrawler
