namespace EzBob.Web.Areas.Broker.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Helpers;
	using System.Web.Mvc;
	using ConfigManager;
	using ExperianLib.Ebusiness;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Utils.MimeTypes;
	using EzBob.Web.Areas.Broker.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EzBob.Web.Infrastructure.Filters;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public partial class BrokerHomeController : Controller {
		public BrokerHomeController() {
			this.m_oServiceClient = new ServiceClient();
			this.m_oHelper = new BrokerHelper(this.m_oServiceClient, ms_oLog);
		} // constructor

		// GET: /Broker/BrokerHome/
		public ViewResult Index(string sourceref = "") {
			var oModel = new BrokerHomeModel();

			if (!string.IsNullOrWhiteSpace(sourceref)) {
				var cookie = new HttpCookie(Constant.SourceRef, sourceref) {
					Expires = DateTime.Now.AddMonths(3),
					HttpOnly = true,
					Secure = true
				};
				Response.Cookies.Add(cookie);
			} // if

			oModel.MessageOnStart = (Session[Constant.Broker.MessageOnStart] ?? string.Empty).ToString()
				.Trim();

			if (!string.IsNullOrWhiteSpace(oModel.MessageOnStart)) {
				oModel.MessageOnStartSeverity = (Session[Constant.Broker.MessageOnStartSeverity] ?? string.Empty).ToString();

				Session[Constant.Broker.MessageOnStart] = null;
				Session[Constant.Broker.MessageOnStartSeverity] = null;
			} // if

			if (User.Identity.IsAuthenticated) {
				oModel.Auth = this.m_oHelper.IsBroker(User.Identity.Name) ? User.Identity.Name : Constant.Broker.Forbidden;

				ms_oLog.Info("Broker page sent to browser with authentication result '{0}' for identified name '{1}'.", oModel.Auth, User.Identity.Name);
			} // if

			oModel.Terms = (Session[Constant.Broker.Terms] ?? string.Empty).ToString()
				.Trim();
			Session[Constant.Broker.Terms] = null;

			if (!string.IsNullOrWhiteSpace(oModel.Terms)) {
				oModel.TermsID = Convert.ToInt32(Session[Constant.Broker.TermsID]);
				Session[Constant.Broker.TermsID] = null;
			} // if

			return View("Index", oModel);
		} // Index

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[CaptchaValidationFilter(Order = 999999)]
		public JsonResult Signup(
			string FirmName,
			string FirmRegNum,
			string ContactName,
			string ContactEmail,
			string ContactMobile,
			string MobileCode,
			string ContactOtherPhone,
			decimal EstimatedMonthlyClientAmount,
			string Password,
			string Password2,
			string FirmWebSite,
			int EstimatedMonthlyAppCount,
			int IsCaptchaEnabled,
			int TermsID
			) {
			string sReferredBy = "";

			if (Request.Cookies.AllKeys.Contains(Constant.SourceRef)) {
				var oCookie = Request.Cookies[Constant.SourceRef];

				if (oCookie != null)
					sReferredBy = oCookie.Value;
			} // if

			ms_oLog.Debug(
				"Broker sign up request:" +
					"\n\tFirm name: {0}" +
					"\n\tFirm reg num: {1}" +
					"\n\tContact person name: {2}" +
					"\n\tContact person email: {3}" +
					"\n\tContact person mobile: {4}" +
					"\n\tMobile code: {5}" +
					"\n\tContact person other phone: {6}" +
					"\n\tEstimated monthly amount: {7}" +
					"\n\tFirm web site URL: {8}" +
					"\n\tEstimated monthly application count: {9}" +
					"\n\tCaptcha enabled: {10}" +
					"\n\tTerms ID: {11}" +
					"\n\tReferred by (sourceref): {12}",
				FirmName,
				FirmRegNum,
				ContactName,
				ContactEmail,
				ContactMobile,
				MobileCode,
				ContactOtherPhone,
				EstimatedMonthlyClientAmount,
				FirmWebSite,
				EstimatedMonthlyAppCount,
				IsCaptchaEnabled == 0 ? "no" : "yes",
				TermsID,
				sReferredBy
				);

			if (!ModelState.IsValid) {
				return new BrokerForJsonResult(string.Join("; ",
					ModelState.Values.SelectMany(x => x.Errors)
						.Select(x => x.ErrorMessage)
					));
			} // if

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn("Sign up request with contact email {0}: already authorised as {1}.", ContactEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerPropertiesActionResult bp;

			try {
				bp = this.m_oServiceClient.Instance.BrokerSignup(
					FirmName,
					FirmRegNum,
					ContactName,
					ContactEmail,
					ContactMobile,
					MobileCode,
					ContactOtherPhone,
					EstimatedMonthlyClientAmount,
					new Password(Password, Password2),
					FirmWebSite,
					EstimatedMonthlyAppCount,
					IsCaptchaEnabled != 0,
					TermsID,
					sReferredBy
					);

				if (!string.IsNullOrEmpty(bp.Properties.ErrorMsg)) {
					ms_oLog.Warn("Failed to sign up as a broker. {0}", bp.Properties.ErrorMsg);
					return new BrokerForJsonResult(bp.Properties.ErrorMsg);
				} // if
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to sign up as a broker.");
				return new BrokerForJsonResult("Registration failed. Please contact customer care.");
			} // try

			BrokerHelper.SetAuth(ContactEmail);

			ms_oLog.Debug("Broker sign up succeeded for: {0}", ContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp.Properties) {
				antiforgery_token = AntiForgery.GetHtml()
					.ToString()
			};
		} // Signup

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Logoff(string sContactEmail) {
			bool bGoodToLogOff =
				string.IsNullOrWhiteSpace(sContactEmail) ||
					(User.Identity.IsAuthenticated && (User.Identity.Name == sContactEmail));

			if (bGoodToLogOff) {
				this.m_oHelper.Logoff(User.Identity.Name, HttpContext);
				return new BrokerForJsonResult {
					antiforgery_token = AntiForgery.GetHtml()
						.ToString()
				};
			} // if

			ms_oLog.Warn(
				"Logoff request with contact email {0} while {1} logged in.",
				sContactEmail,
				User.Identity.IsAuthenticated ? "broker " + User.Identity.Name + " is" : "not"
				);

			return new BrokerForJsonResult(bExplicitSuccess: false);
		} // Logoff

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Login(string LoginEmail, string LoginPassword) {
			ms_oLog.Debug("Broker login request: {0}", LoginEmail);

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn("Login request with contact email {0}: already authorised as {1}.", LoginEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerProperties bp = this.m_oHelper.TryLogin(LoginEmail, LoginPassword);

			if (bp == null)
				return new BrokerForJsonResult("Failed to log in.");

			ms_oLog.Debug("Broker login succeeded for: {0}", LoginEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp) {
				antiforgery_token = AntiForgery.GetHtml()
					.ToString()
			};
		} // Login

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AcceptTerms(int nTermsID, string sContactEmail) {
			ms_oLog.Debug("Broker accept terms request for contact email {0} and terms id {1}...", sContactEmail, nTermsID);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Accept terms", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				this.m_oServiceClient.Instance.BrokerAcceptTerms(nTermsID, sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to save terms acceptance request for contact email {0} and terms id {1}.", sContactEmail, nTermsID);
				return new BrokerForJsonResult("Failed to save terms acceptance.");
			} // try

			ms_oLog.Debug("Broker accept terms request for contact email {0} and terms id {1} complete.", sContactEmail, nTermsID);

			return new BrokerForJsonResult();
		} // AcceptTerms

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadSignedTerms(string sContactEmail) {
			ms_oLog.Debug("Broker load signed terms request for contact email {0}...", sContactEmail);

			var oIsAuthResult = IsAuth<SignedTermsBrokerForJsonResult>("Accept terms", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			StringListActionResult slar;

			try {
				slar = this.m_oServiceClient.Instance.BrokerLoadSignedTerms(sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load signed terms for contact email {0}.", sContactEmail);
				return new SignedTermsBrokerForJsonResult("Failed to load signed terms.");
			} // try

			ms_oLog.Debug("Broker load signed terms request for contact email {0}.", sContactEmail);

			return new SignedTermsBrokerForJsonResult(sTerms: slar.Records[0], sSignedTime: slar.Records[1]);
		} // LoadSignedTerms

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RestorePassword(string ForgottenMobile, string ForgottenMobileCode) {
			ms_oLog.Debug("Broker restore password request: phone # {0} with code {1}", ForgottenMobile, ForgottenMobileCode);

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn("Request with mobile phone {0} and code {1}: already authorised as {2}.", ForgottenMobile, ForgottenMobileCode, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			try {
				this.m_oServiceClient.Instance.BrokerRestorePassword(ForgottenMobile, ForgottenMobileCode);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to restore password for a broker with phone # {0}.", ForgottenMobile);
				return new BrokerForJsonResult("Failed to restore password.");
			} // try

			ms_oLog.Debug("Broker restore password succeded for phone # {0}", ForgottenMobile);

			return new BrokerForJsonResult();
		} // RestorePassword

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadProperties(string sContactEmail) {
			ms_oLog.Debug("Broker load properties request for contact email {0}", sContactEmail);

			var oIsAuthResult = IsAuth<PropertiesBrokerForJsonResult>("Load properties", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerPropertiesActionResult oResult;

			try {
				oResult = this.m_oServiceClient.Instance.BrokerLoadOwnProperties(sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load properties request for contact email {0}", sContactEmail);
				return new PropertiesBrokerForJsonResult("Failed to load broker properties.");
			} // try

			ms_oLog.Debug("Broker load customers properties for contact email {0} complete.", sContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: oResult.Properties);
		} // LoadProperties

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomers(string sContactEmail) {
			ms_oLog.Debug("Broker load customers request for contact email {0}", sContactEmail);

			var oIsAuthResult = IsAuth<CustomerListBrokerForJsonResult>("Load customers", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomersActionResult oResult;

			try {
				oResult = this.m_oServiceClient.Instance.BrokerLoadCustomerList(sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load customers request for contact email {0}", sContactEmail);
				return new CustomerListBrokerForJsonResult("Failed to load customer list.");
			} // try

			ms_oLog.Debug("Broker load customers request for contact email {0} complete.", sContactEmail);

			return new CustomerListBrokerForJsonResult(oCustomers: oResult.Customers);
		} // LoadCustomers

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomerDetails(string sCustomerID, string sContactEmail) {
			ms_oLog.Debug("Broker load customer details request for customer {1} and contact email {0}", sContactEmail, sCustomerID);

			var oIsAuthResult = IsAuth<CustomerDetailsBrokerForJsonResult>("Load customer details for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomerDetailsActionResult oDetails;

			try {
				oDetails = this.m_oServiceClient.Instance.BrokerLoadCustomerDetails(sCustomerID, sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load customer details request for customer {1} and contact email {0}", sContactEmail, sCustomerID);
				return new CustomerDetailsBrokerForJsonResult("Failed to load customer details.");
			} // try

			ms_oLog.Debug("Broker load customer details request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new CustomerDetailsBrokerForJsonResult(oDetails: oDetails.Data, oPotentialSigners: oDetails.PotentialSigners);
		} // LoadCustomerDetails

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadStaticData() {
			ms_oLog.Debug("Broker loading CRM details started...");

			BrokerStaticDataActionResult oResult;

			try {
				oResult = this.m_oServiceClient.Instance.BrokerLoadStaticData(false);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Broker loading static data failed.");

				oResult = new BrokerStaticDataActionResult {
					MaxPerNumber = 3,
					MaxPerPage = 10,
					Files = new FileDescription[0],
					Terms = "",
					TermsID = 0,
				};
			} // try

			ms_oLog.Debug("Broker loading CRM details complete.");

			return Json(new {
				success = true,
				data = oResult,
			}, JsonRequestBehavior.AllowGet);
		} // LoadStaticData

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveCrmEntry(string type, int action, int status, string comment, string customerId, string sContactEmail) {
			ms_oLog.Debug(
				"\nBroker saving CRM entry started:" +
					"\n\ttype: {0}" +
					"\n\taction: {1}" +
					"\n\tstatus: {2}" +
					"\n\tcustomer id: {3}" +
					"\n\tcontact email: {4}" +
					"\n\tcomment: {5}\n",
				type, action, status, customerId, sContactEmail, comment
				);

			BrokerForJsonResult oIsAuthResult = IsAuth("Save CRM entry for customer " + customerId, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			StringActionResult oResult;

			try {
				oResult = this.m_oServiceClient.Instance.BrokerSaveCrmEntry(type, action, status, comment, customerId, sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e,
					"\nBroker saving CRM entry failed for:" +
						"\n\ttype: {0}" +
						"\n\taction: {1}" +
						"\n\tstatus: {2}" +
						"\n\tcustomer id: {3}" +
						"\n\tcontact email: {4}",
					type, action, status, customerId, sContactEmail
					);

				return new BrokerForJsonResult("Failed to save CRM entry.");
			} // try

			ms_oLog.Debug(
				"\nBroker saving CRM entry {5} for:" +
					"\n\ttype: {0}" +
					"\n\taction: {1}" +
					"\n\tstatus: {2}" +
					"\n\tcustomer id: {3}" +
					"\n\tcontact email: {4}\n" +
					"\n\terror message: {6}\n",
				type, action, status, customerId, sContactEmail,
				string.IsNullOrWhiteSpace(oResult.Value) ? "complete" : "failed",
				string.IsNullOrWhiteSpace(oResult.Value) ? "no error" : oResult.Value
				);

			return new BrokerForJsonResult(oResult.Value);
		} // SaveCrmEntry

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomerFiles(string sCustomerID, string sContactEmail) {
			ms_oLog.Debug("Broker load customer files request for customer {1} and contact email {0}", sContactEmail, sCustomerID);

			var oIsAuthResult = IsAuth<FileListBrokerForJsonResult>("Load customer files for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomerFilesActionResult oFiles;

			try {
				oFiles = this.m_oServiceClient.Instance.BrokerLoadCustomerFiles(sCustomerID, sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load customer files request for customer {1} and contact email {0}", sContactEmail, sCustomerID);
				return new FileListBrokerForJsonResult("Failed to load customer files.");
			} // try

			ms_oLog.Debug("Broker load customer files request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new FileListBrokerForJsonResult(oFileList: oFiles.Files);
		} // LoadCustomerFiles

		[HttpPost]
		public JsonResult HandleUploadFile() {
			string sContactEmail = Request.Headers["ezbob-broker-contact-email"];

			string sCustomerID = Request.Headers["ezbob-broker-customer-id"];

			ms_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0}", sContactEmail, sCustomerID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Upload customer file for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			var nFileCount = Request.Files.Count;

			var oErrorList = new List<string>();

			OneUploadLimitation oLimitations = CurrentValues.Instance.GetUploadLimitations("BrokerHome", "HandleUploadFile");

			for (int i = 0; i < nFileCount; i++) {
				HttpPostedFileBase oFile = Request.Files[i];

				if (oFile == null) {
					ms_oLog.Alert("File object #{0} out of {1} is null.", (i + 1), nFileCount);
					oErrorList.Add("Failed to upload file #" + (i + 1));
					continue;
				} // if

				var oFileContents = new byte[oFile.InputStream.Length];

				int nRead = oFile.InputStream.Read(oFileContents, 0, oFile.ContentLength);

				if (nRead != oFile.ContentLength) {
					oErrorList.Add("Failed to fully file #" + (i + 1) + ": " + oFile.FileName);
					ms_oLog.Alert(
						"Failed to fully read file #{0}: {2} out of {1}; only {3} bytes out of {4} have been read.",
						(i + 1), nFileCount, oFile.FileName, nRead, oFile.ContentLength
						);
					continue;
				} // if

				string sMimeType = oLimitations.DetectFileMimeType(oFileContents, oFile.FileName, oLog: ms_oLog);

				ms_oLog.Debug(
					"File #{0} out of {1}: {2}; file size is {3} bytes, detected MIME type: {4}",
					(i + 1), nFileCount, oFile.FileName, nRead, sMimeType
					);

				if (string.IsNullOrWhiteSpace(sMimeType))
					oErrorList.Add("Not saving file #" + (i + 1) + ": " + oFile.FileName + " because it has unsupported MIME type.");
				else {
					try {
						this.m_oServiceClient.Instance.BrokerSaveUploadedCustomerFile(sCustomerID, sContactEmail, oFileContents, oFile.FileName);
					} catch (Exception e) {
						ms_oLog.Alert(e, "Failed to save file #{0}: {2} out of {1}.", (i + 1), nFileCount, oFile.FileName);
						oErrorList.Add("Failed to save file #" + (i + 1) + ": " + oFile.FileName);
					} // try
				} // if
			} // for each file

			ms_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new BrokerForJsonResult(oErrorList.Count == 0 ? string.Empty : string.Join(" ", oErrorList));
		} // HandleUploadFile

		[HttpGet]
		public FileResult DownloadCustomerFile(string sCustomerID, string sContactEmail, int nFileID) {
			ms_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Download customer file for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				throw new Exception(oIsAuthResult.error);

			BrokerCustomerFileContentsActionResult oFile;

			try {
				oFile = this.m_oServiceClient.Instance.BrokerDownloadCustomerFile(sCustomerID, sContactEmail, nFileID);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to download customer file for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);
				throw new Exception("Failed to download requested file.");
			} // try

			if (string.IsNullOrWhiteSpace(oFile.Name)) {
				ms_oLog.Alert("Could not download customer file for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);
				throw new Exception("Failed to download requested file.");
			} // if

			ms_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2} complete.", sContactEmail, sCustomerID, nFileID);

			string sFileExt = string.Empty;

			int nLastDotPos = oFile.Name.LastIndexOf('.');

			if ((nLastDotPos > -1) && (nLastDotPos < oFile.Name.Length - 1))
				sFileExt = oFile.Name.Substring(nLastDotPos);

			return new FileContentResult(oFile.Contents, new MimeTypeResolver()[sFileExt]) {
				FileDownloadName = oFile.Name,
			};
		} // DownloadCustomerFile

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult DeleteCustomerFiles(string sCustomerID, string sContactEmail, int[] aryFileIDs) {
			string sErrorMsg = null;
			string sFileIDList = string.Empty;

			if (aryFileIDs == null)
				sErrorMsg = "list of file ids is null after parsing";
			else if (aryFileIDs.Length < 1)
				sErrorMsg = "list of file ids is empty after parsing";
			else
				sFileIDList = string.Join(", ", aryFileIDs);

			if (!string.IsNullOrWhiteSpace(sErrorMsg)) {
				ms_oLog.Alert("Failed to delete customer files request for customer {1} and contact email {0}: {2}.", sContactEmail, sCustomerID, sErrorMsg);
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // if

			ms_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, sCustomerID, sFileIDList);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Delete customer files for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				this.m_oServiceClient.Instance.BrokerDeleteCustomerFiles(sCustomerID, sContactEmail, aryFileIDs);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, sCustomerID, sFileIDList);
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // try

			ms_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2} complete.", sContactEmail, sCustomerID, sFileIDList);

			return new BrokerForJsonResult();
		} // DeleteCustomerFiles

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddLead(string LeadFirstName, string LeadLastName, string LeadEmail, string LeadAddMode, string ContactEmail) {
			ms_oLog.Debug("Broker add lead request for contact email {0}: {1} {2}, {3} - {4}.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);

			var oIsAuthResult = IsAuth("Add lead", ContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				this.m_oServiceClient.Instance.BrokerAddCustomerLead(LeadFirstName, LeadLastName, LeadEmail, LeadAddMode, ContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to add lead for contact email {0}: {1} {2}, {3} - {4}.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);
				return new BrokerForJsonResult("Failed to add customer lead.");
			} // try

			ms_oLog.Debug("Broker add lead request for contact email {0}: {1} {2}, {3} - {4} complete.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);

			return new BrokerForJsonResult();
		} // AddLead

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SendInvitation(int nLeadID, string sContactEmail) {
			ms_oLog.Debug("Broker send invitation request for contact email {0} and lead id {1}.", sContactEmail, nLeadID);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Send invitation", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				this.m_oServiceClient.Instance.BrokerLeadSendInvitation(nLeadID, sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to send invitation request for contact email {0} and lead id {1}.", sContactEmail, nLeadID);
				return new BrokerForJsonResult("Failed to send an invitation.");
			} // try

			ms_oLog.Debug("Broker send invitation request for contact email {0} and lead id {1} complete.", sContactEmail, nLeadID);

			return new BrokerForJsonResult();
		} // SendInvitation

		[HttpGet]
		public System.Web.Mvc.ActionResult FillWizard(int? nLeadID, string sLeadEmail, string sContactEmail) {
			nLeadID = nLeadID ?? 0;

			ms_oLog.Debug("Broker fill wizard request for contact email {0} and lead id {1} lead email {2}.", sContactEmail, nLeadID, sLeadEmail);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Send invitation", sContactEmail);
			if (oIsAuthResult != null) {
				Session[Constant.Broker.MessageOnStart] = oIsAuthResult.error;
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new {
					Area = "Broker",
				});
			} // if

			if ((nLeadID > 0) && !string.IsNullOrWhiteSpace(sLeadEmail)) {
				ms_oLog.Warn("Both lead id ({0}) and lead email ({1}) specified while there can be only one.", nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new {
					Area = "Broker",
				});
			} // if

			BrokerLeadDetailsActionResult bld;

			try {
				bld = this.m_oServiceClient.Instance.BrokerLeadCanFillWizard(nLeadID.Value, sLeadEmail, sContactEmail);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to process fill wizard request for contact email {0} and lead id {1} lead email {2}.", sContactEmail, nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new {
					Area = "Broker",
				});
			} // try

			if (bld.LeadID < 1) {
				ms_oLog.Warn("Validated lead id is {0}. Source lead id is {1} lead email {2}.", bld.LeadID, nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new {
					Area = "Broker",
				});
			} // if

			this.m_oHelper.Logoff(User.Identity.Name, HttpContext);
			if (bld.CustomerID > 0)
				BrokerHelper.SetAuth(bld.LeadEmail, HttpContext, "Customer");

			// ReSharper disable ObjectCreationAsStatement
			// This constructor sets Session data.
			new WizardBrokerLeadModel(
				Session,
				bld.LeadID,
				bld.LeadEmail,
				bld.FirstName,
				bld.LastName,
				true
				);
			// ReSharper restore ObjectCreationAsStatement

			ms_oLog.Debug("Broker fill wizard request for contact email {0} and lead id {1} lead email {2} complete.", sContactEmail, nLeadID, sLeadEmail);

			return RedirectToAction("Index", "Wizard", new {
				Area = "Customer"
			});
		} // FillWizard

		[HttpGet]
		public System.Web.Mvc.ActionResult FinishWizardLater() {
			var blm = new WizardBrokerLeadModel(Session);

			ms_oLog.Debug("Broker fill wizard later request: {0}", blm);

			if (blm.BrokerFillsForCustomer) {
				StringActionResult sar = null;

				try {
					sar = this.m_oServiceClient.Instance.BrokerBackFromCustomerWizard(blm.LeadID);
				} catch (Exception e) {
					ms_oLog.Warn(e, "Failed to retrieve broker details, falling back to customer's dashboard.");
				} // try

				if (sar == null)
					ms_oLog.Debug("Failed to retrieve broker details.");
				else {
					BrokerHelper.SetAuth(null, HttpContext);

					ms_oLog.Debug("Restoring broker identity after filling customer wizard: '{0}'.", sar.Value);
					BrokerHelper.SetAuth(sar.Value);

					blm.Unset();
					return RedirectToAction("Index", "BrokerHome", new {
						Area = "Broker"
					});
				} // if
			} // if

			ms_oLog.Debug("Broker fill wizard later request failed, redirecting back to customer wizard.");

			blm.Unset();
			return RedirectToAction("Index", "Wizard", new {
				Area = "Customer"
			});
		} // FinishWizardLater

		public System.Web.Mvc.ActionResult DownloadFile(string fid) {
			if (string.IsNullOrWhiteSpace(fid)) {
				ms_oLog.Warn("Broker download file request: file with no id.");
				return HttpNotFound();
			} // if

			string sFileName = fid.Trim();

			var oFiles = new MarketingFiles(this.m_oServiceClient);

			ms_oLog.Debug("Broker download file request: file with id {0}.", sFileName);

			FileDescription fd = oFiles.Find(sFileName);

			if (fd != null) {
				string sPath = System.Web.HttpContext.Current.Server.MapPath("~" + BrokerHomeModel.MarketingFileLocation + fd.FileName);

				ms_oLog.Debug("Broker download file request: found file with id {0} of type {1} as {2}.", sFileName, fd.MimeType, sPath);

				return File(sPath, fd.MimeType, fd.FileName);
			} // if

			ms_oLog.Debug("Broker download file request: file with id {0} was not found.", sFileName);

			return HttpNotFound();
		} // DownloadFile

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UpdatePassword(string ContactEmail, string OldPassword, string NewPassword, string NewPassword2) {
			ms_oLog.Debug("Broker update password request for contact email {0}", ContactEmail);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Update password", ContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			if (ReferenceEquals(OldPassword, null) || ReferenceEquals(NewPassword, null) || ReferenceEquals(NewPassword2, null)) {
				ms_oLog.Warn("Cannot update password for contact email {0}: one of passwords not specified.", ContactEmail);
				return new BrokerForJsonResult("Cannot update password: some required fields are missing.");
			} // if

			if (NewPassword != NewPassword2) {
				ms_oLog.Warn("Cannot update password: passwords do not match.");
				return new BrokerForJsonResult("Cannot update password: passwords do not match.");
			} // if

			if (NewPassword == OldPassword) {
				ms_oLog.Warn("Cannot update password: new password is equal to the old one.");
				return new BrokerForJsonResult("Cannot update password: new password is equal to the old one.");
			} // if

			ActionMetaData oResult;

			try {
				oResult = this.m_oServiceClient.Instance.BrokerUpdatePassword(
					ContactEmail,
					new Password(OldPassword),
					new Password(NewPassword, NewPassword2)
					);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update password for contact email {0}", ContactEmail);
				return new BrokerForJsonResult("Failed to update password.");
			} // try

			if (oResult == null) {
				ms_oLog.Warn("Failed to update password for contact email {0}", ContactEmail);
				return new BrokerForJsonResult("Failed to update password.");
			} // if

			ms_oLog.Debug("Broker update password request for contact email {0} complete.", ContactEmail);

			return new BrokerForJsonResult();
		} // UpdatePassword

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveExperianDirector(
			string sCustomerID,
			string sContactEmail,
			int directorID,
			string email,
			string mobilePhone,
			string line1,
			string line2,
			string line3,
			string town,
			string county,
			string postcode
			) {
			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Save Experian director details for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			ms_oLog.Debug("Saving Experian director (BrokerHome controller, broker {9}): {0}: {1} {2}, {3} {4} {5} {6} {7} {8}",
				directorID,
				email,
				mobilePhone,
				line1,
				line2,
				line3,
				town,
				county,
				postcode,
				sContactEmail
				);

			var m = new Esigner {
				DirectorID = directorID,
				Email = (email ?? string.Empty).Trim(),
				MobilePhone = (mobilePhone ?? string.Empty).Trim(),
				Line1 = (line1 ?? string.Empty).Trim(),
				Line2 = (line2 ?? string.Empty).Trim(),
				Line3 = (line3 ?? string.Empty).Trim(),
				Town = (town ?? string.Empty).Trim(),
				County = (county ?? string.Empty).Trim(),
				Postcode = (postcode ?? string.Empty).Trim(),
			};

			string sValidation = m.ValidateExperianDirectorDetails();

			if (!string.IsNullOrWhiteSpace(sValidation))
				return new BrokerForJsonResult(sValidation);

			try {
				this.m_oServiceClient.Instance.UpdateExperianDirectorDetails(null, null, m);
			} catch (Exception e) {
				ms_oLog.Warn(e, "Failed to save Experian director details.");
				return new BrokerForJsonResult("Failed to save director details.");
			} // try

			return new BrokerForJsonResult();
		} // SaveExperianDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetOffer(GetInstantOfferModel model, CompanyInfo company) {
			var context = ObjectFactory.GetInstance<IWorkplaceContext>();
			var response = this.m_oServiceClient.Instance.BrokerInstantOffer(new BrokerInstantOfferRequest {
				Created = DateTime.UtcNow,
				BrokerId = context.UserId,
				CompanyNameNumber = model.CompanyNameNumber,
				AnnualTurnover = model.AnnualTurnover,
				AnnualProfit = model.AnnualProfit,
				NumOfEmployees = model.NumOfEmployees,
				MainApplicantCreditScore = model.MainApplicantCreditScore,
				IsHomeOwner = model.IsHomeOwner,
				ExperianRefNum = company.BusRefNum,
				ExperianCompanyName = company.BusName,
				ExperianCompanyLegalStatus = company.LegalStatus,
				ExperianCompanyPostcode = company.PostCode
			});

			var loanTypeRepository = ObjectFactory.GetInstance<LoanTypeRepository>();
			var loanSourceRepository = ObjectFactory.GetInstance<LoanSourceRepository>();
			var loanBuilder = ObjectFactory.GetInstance<LoanBuilder>();
			var aprCalc = ObjectFactory.GetInstance<APRCalculator>();

			var cr = new CashRequest {
				ApprovedRepaymentPeriod = response.Response.RepaymentPeriod,
				InterestRate = response.Response.InterestRate,
				LoanType = loanTypeRepository.Get(response.Response.LoanTypeId),
				LoanSource = loanSourceRepository.Get(response.Response.LoanSourceId),
				ManagerApprovedSum = response.Response.ApprovedSum,
				UseBrokerSetupFee = response.Response.UseBrokerSetupFee,
				UseSetupFee = response.Response.UseSetupFee,
				RepaymentPeriod = response.Response.RepaymentPeriod,
				LoanLegals = new List<LoanLegal>()
			};

			var loan = loanBuilder.CreateNewLoan(cr,
				cr.ApprovedSum(),
				DateTime.UtcNow,
				cr.ApprovedRepaymentPeriod.HasValue ? cr.ApprovedRepaymentPeriod.Value : 12);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();

			var apr = loan.LoanAmount == 0 ? 0 : aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

			var loanOffer = LoanOffer.InitFromLoan(loan, apr, null, cr);

			return Json(loanOffer, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult TargetBusiness(string companyNameNumber) {
			if (string.IsNullOrEmpty(companyNameNumber))
				return new BrokerForJsonResult("Company name/number not provided");

			string companyName = "";
			string companyNumber = "";
			var legalStatus = TargetResults.LegalStatus.DontCare;
			//company ref num is alpha numeric 8 chars length string, digits only or 2 letters and 6 digits.
			if (companyNameNumber.Length <= 8 &&
				(Regex.IsMatch(companyNameNumber, @"^\d+$") || Regex.IsMatch(companyNameNumber, @"^[a-zA-Z]{2}\d+$")))
				companyNumber = companyNameNumber;
			else
				companyName = companyNameNumber;

			if (companyNameNumber.ToLowerInvariant()
				.Contains("ltd") || companyNameNumber.ToLowerInvariant()
					.Contains("limited"))
				legalStatus = TargetResults.LegalStatus.Limited;

			try {
				var service = new EBusinessService(m_oDB);

				//TODO remove the hard coded targeting
				var result =
					new TargetResults(
						"<?xml version='1.0' standalone='yes' ?><GEODS  exp_cid='cPek' > <REQUEST type='BINSIGHT' subtype='CALLBUR' EXP_ExperianRef='' success='Y' timestamp='Tue, 26 Aug 2014 at 3:13 PM' id='cPek'><DK10 seq='00'><CICSREGION>CIXF    </CICSREGION><MFDATE-YYYY>2014</MFDATE-YYYY><MFDATE-MM>08</MFDATE-MM><MFDATE-DD>26</MFDATE-DD><TIME>151324</TIME></DK10><DK12 seq='00'><ACCESSITEMSCOUNT>008</ACCESSITEMSCOUNT><ACCESSITEMS><ITEMCODE>BTAR</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>BTPC</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>BTPH</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CDEL</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CDEN</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CDE6</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>SHNA</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CD3L</ITEMCODE></ACCESSITEMS></DK12><DG02 seq='01'><ERRORS><ERRORCODE>W006</ERRORCODE></ERRORS><ERRORS></ERRORS><ERRORS></ERRORS><NUMMATCHES>000</NUMMATCHES><LENLOCALITIES>0000</LENLOCALITIES></DG02><DT10 seq='01'><NUMMATCHES>2</NUMMATCHES><TOOMANYCANDIDATES>N</TOOMANYCANDIDATES><TOOMANYMATCHES>N</TOOMANYMATCHES><BUSREFNUM>06262589</BUSREFNUM><BUSNAME>06262589</BUSNAME></DT10><DT11 seq='01'><LEGALSTATUS>L</LEGALSTATUS><BUSINESSSTATUS>A</BUSINESSSTATUS><MATCHSCORE>100</MATCHSCORE><BUSREFNUM>06262589</BUSREFNUM><BUSNAME>EPATIENT NETWORK LIMITED</BUSNAME><ADDRLINE1>PO BOX 1295 20 STATION ROAD</ADDRLINE1><ADDRLINE2>GERRARDS CROSS</ADDRLINE2><ADDRLINE3>BUCKINGHAMSHIRE</ADDRLINE3><POSTCODE>SL9 8EL</POSTCODE><SICCODETYPE>B</SICCODETYPE><SICCODE>7484</SICCODE><SICCODEDESC>OTHER BUSINESS ACTIVITIES</SICCODEDESC></DT11><DT11 seq='01'><LEGALSTATUS>N</LEGALSTATUS><BUSINESSSTATUS>A</BUSINESSSTATUS><MATCHSCORE>100</MATCHSCORE><BUSREFNUM>06262589</BUSREFNUM><BUSNAME>LITTLEPORT RANGERS FOOTBALL CLUB</BUSNAME><ADDRLINE1>79 FISHERS BANK</ADDRLINE1><ADDRLINE2>LITTLEPORT</ADDRLINE2><ADDRLINE3>ELY</ADDRLINE3><ADDRLINE4>CAMBRIDGESHIRE</ADDRLINE4><POSTCODE>CB6 1LN</POSTCODE></DT11><DM12 seq='00'><NOCCOUNT>0</NOCCOUNT></DM12><DK01 seq='01'><APPNAME>XML8255   </APPNAME><VERSIONMAJOR>0100</VERSIONMAJOR><VERSIONMINOR>0100</VERSIONMINOR></DK01><DK02 seq='01'><USERDEPT>0</USERDEPT><VER0101END>#</VER0101END></DK02><DK03 seq='01'><MESSAGETYPE>BTGR</MESSAGETYPE><MESSAGESUBTYPE>0001</MESSAGESUBTYPE><MESSAGEID>00        </MESSAGEID></DK03><DT01 seq='01'><ADDRINSTANCE>01</ADDRINSTANCE><SEARCHCORP>Y</SEARCHCORP><EXLUDEDISSCORP>N</EXLUDEDISSCORP><SEARCHNONCORP>Y</SEARCHNONCORP><SINGLESHOT>N</SINGLESHOT><EXACTNAMEMATCH>N</EXACTNAMEMATCH><BUSREFNUM>06262589</BUSREFNUM><BUSNAME>06262589</BUSNAME></DT01><ADDR seq='01'><FORMATCODE>UKNLTD  </FORMATCODE><DATACOUNT>3</DATACOUNT><ELEMENTCOUNT>8</ELEMENTCOUNT><DATAITEMS><DATAITEM>NULL</DATAITEM></DATAITEMS><DATAITEMS></DATAITEMS><DATAITEMS></DATAITEMS></ADDR></REQUEST></GEODS>");
				//service.TargetBusiness(companyName, null, 0, legalStatus, companyNumber);

				return Json(result.Targets, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Target Business failed.");
				throw;
			} // try
		}
	} // class BrokerHomeController
} // namespace
