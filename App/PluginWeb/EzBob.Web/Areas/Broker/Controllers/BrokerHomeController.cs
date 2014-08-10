namespace EzBob.Web.Areas.Broker.Controllers {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Helpers;
	using System.Web.Mvc;
	using Code;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Infrastructure.csrf;
	using Ezbob.Utils;
	using Infrastructure.Attributes;
	using Infrastructure.Filters;
	using Models;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	#endregion using

	public partial class BrokerHomeController : Controller {
		#region constructor

		public BrokerHomeController() {
			m_oServiceClient = new ServiceClient();
			m_oHelper = new BrokerHelper(m_oServiceClient, ms_oLog);
		} // constructor

		#endregion constructor

		#region action Index (default)

		// GET: /Broker/BrokerHome/
		public ViewResult Index(string sourceref = "") {
			var oModel = new BrokerHomeModel();

			if (!string.IsNullOrWhiteSpace(sourceref)) {
				var cookie = new HttpCookie(Constant.SourceRef, sourceref) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			} // if

			oModel.MessageOnStart = (Session[Constant.Broker.MessageOnStart] ?? string.Empty).ToString().Trim();

			if (!string.IsNullOrWhiteSpace(oModel.MessageOnStart)) {
				oModel.MessageOnStartSeverity = (Session[Constant.Broker.MessageOnStartSeverity] ?? string.Empty).ToString();

				Session[Constant.Broker.MessageOnStart] = null;
				Session[Constant.Broker.MessageOnStartSeverity] = null;
			} // if

			if (User.Identity.IsAuthenticated) {
				oModel.Auth = m_oHelper.IsBroker(User.Identity.Name) ? User.Identity.Name : Constant.Broker.Forbidden;

				ms_oLog.Info("Broker page sent to browser with authentication result '{0}' for identified name '{1}'.", oModel.Auth, User.Identity.Name);
			} // if

			return View("Index", oModel);
		} // Index

		#endregion action Index (default)

		#region action Signup

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
					ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)
				));
			} // if

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn("Sign up request with contact email {0}: already authorised as {1}.", ContactEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerPropertiesActionResult bp;

			try {
				bp = m_oServiceClient.Instance.BrokerSignup(
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
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to sign up as a broker.");
				return new BrokerForJsonResult("Registration failed. Please contact customer care.");
			} // try

			BrokerHelper.SetAuth(ContactEmail);

			ms_oLog.Debug("Broker sign up succeeded for: {0}", ContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp.Properties) { antiforgery_token = AntiForgery.GetHtml().ToString() };
		} // Signup

		#endregion action Signup

		#region action Logoff

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Logoff(string sContactEmail) {
			bool bGoodToLogOff =
				string.IsNullOrWhiteSpace(sContactEmail) ||
				(User.Identity.IsAuthenticated && (User.Identity.Name == sContactEmail));

			if (bGoodToLogOff) {
				m_oHelper.Logoff(User.Identity.Name, HttpContext);
				return new BrokerForJsonResult { antiforgery_token = AntiForgery.GetHtml().ToString() };
			} // if

			ms_oLog.Warn(
				"Logoff request with contact email {0} while {1} logged in.",
				sContactEmail,
				User.Identity.IsAuthenticated ? "broker " + User.Identity.Name + " is" : "not"
			);

			return new BrokerForJsonResult(bExplicitSuccess: false);
		} // Logoff

		#endregion action Logoff

		#region action Login

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Login(string LoginEmail, string LoginPassword) {
			ms_oLog.Debug("Broker login request: {0}", LoginEmail);

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn("Login request with contact email {0}: already authorised as {1}.", LoginEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerProperties bp = m_oHelper.TryLogin(LoginEmail, LoginPassword);

			if (bp == null)
				return new BrokerForJsonResult("Failed to log in.");

			ms_oLog.Debug("Broker login succeeded for: {0}", LoginEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp) { antiforgery_token = AntiForgery.GetHtml().ToString() };
		} // Login

		#endregion action Login

		#region action RestorePassword

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
				m_oServiceClient.Instance.BrokerRestorePassword(ForgottenMobile, ForgottenMobileCode);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to restore password for a broker with phone # {0}.", ForgottenMobile);
				return new BrokerForJsonResult("Failed to restore password.");
			} // try

			ms_oLog.Debug("Broker restore password succeded for phone # {0}", ForgottenMobile);

			return new BrokerForJsonResult();
		} // RestorePassword

		#endregion action RestorePassword

		#region action LoadProperties

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
				oResult = m_oServiceClient.Instance.BrokerLoadOwnProperties(sContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load properties request for contact email {0}", sContactEmail);
				return new PropertiesBrokerForJsonResult("Failed to load broker properties.");
			} // try

			ms_oLog.Debug("Broker load customers properties for contact email {0} complete.", sContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: oResult.Properties);
		} // LoadProperties

		#endregion action LoadProperties

		#region action LoadCustomers

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
				oResult = m_oServiceClient.Instance.BrokerLoadCustomerList(sContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load customers request for contact email {0}", sContactEmail);
				return new CustomerListBrokerForJsonResult("Failed to load customer list.");
			} // try

			ms_oLog.Debug("Broker load customers request for contact email {0} complete.", sContactEmail);

			return new CustomerListBrokerForJsonResult(oCustomers: oResult.Customers);
		} // LoadCustomers

		#endregion action LoadCustomers

		#region action LoadCustomerDetails

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
				oDetails = m_oServiceClient.Instance.BrokerLoadCustomerDetails(sCustomerID, sContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load customer details request for customer {1} and contact email {0}", sContactEmail, sCustomerID);
				return new CustomerDetailsBrokerForJsonResult("Failed to load customer details.");
			} // try

			ms_oLog.Debug("Broker load customer details request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new CustomerDetailsBrokerForJsonResult(oDetails: oDetails.Data, oPotentialSigners: oDetails.PotentialSigners);
		} // LoadCustomerDetails

		#endregion action LoadCustomerDetails

		#region action LoadStaticData

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadStaticData() {
			ms_oLog.Debug("Broker loading CRM details started...");

			BrokerStaticDataActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadStaticData(false);
			}
			catch (Exception e) {
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

			return Json(new { success = true, data = oResult, }, JsonRequestBehavior.AllowGet);
		} // LoadStaticData

		#endregion action LoadStaticData

		#region action SaveCrmEntry

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
				oResult = m_oServiceClient.Instance.BrokerSaveCrmEntry(type, action, status, comment, customerId, sContactEmail);
			}
			catch (Exception e) {
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

		#endregion action SaveCrmEntry

		#region action LoadCustomerFiles

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
				oFiles = m_oServiceClient.Instance.BrokerLoadCustomerFiles(sCustomerID, sContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to load customer files request for customer {1} and contact email {0}", sContactEmail, sCustomerID);
				return new FileListBrokerForJsonResult("Failed to load customer files.");
			} // try

			ms_oLog.Debug("Broker load customer files request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new FileListBrokerForJsonResult(oFileList: oFiles.Files);
		} // LoadCustomerFiles

		#endregion action LoadCustomerFiles

		#region action HandleUploadFile

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

				ms_oLog.Debug(
					"File #{0}: {2} out of {1}; file size is {3} bytes.",
					(i + 1), nFileCount, oFile.FileName, nRead
				);

				try {
					m_oServiceClient.Instance.BrokerSaveUploadedCustomerFile(sCustomerID, sContactEmail, oFileContents, oFile.FileName);
				}
				catch (Exception e) {
					ms_oLog.Alert(e, "Failed to save file #{0}: {2} out of {1}.", (i + 1), nFileCount, oFile.FileName);
					oErrorList.Add("Failed to save file #" + (i + 1) + ": " + oFile.FileName);
				} // try
			} // for each file

			ms_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new BrokerForJsonResult(oErrorList.Count == 0 ? string.Empty : string.Join(" ", oErrorList));
		} // HandleUploadFile

		#endregion action HandleUploadFile

		#region action DownloadCustomerFile

		[HttpGet]
		public FileResult DownloadCustomerFile(string sCustomerID, string sContactEmail, int nFileID) {
			ms_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Download customer file for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				throw new Exception(oIsAuthResult.error);

			BrokerCustomerFileContentsActionResult oFile;

			try {
				oFile = m_oServiceClient.Instance.BrokerDownloadCustomerFile(sCustomerID, sContactEmail, nFileID);
			}
			catch (Exception e) {
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

		#endregion action DownloadCustomerFile

		#region action DeleteCustomerFiles

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
				m_oServiceClient.Instance.BrokerDeleteCustomerFiles(sCustomerID, sContactEmail, aryFileIDs);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, sCustomerID, sFileIDList);
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // try

			ms_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2} complete.", sContactEmail, sCustomerID, sFileIDList);

			return new BrokerForJsonResult();
		} // DeleteCustomerFiles

		#endregion action DeleteCustomerFiles

		#region action AddLead

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddLead(string LeadFirstName, string LeadLastName, string LeadEmail, string LeadAddMode, string ContactEmail) {
			ms_oLog.Debug("Broker add lead request for contact email {0}: {1} {2}, {3} - {4}.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);

			var oIsAuthResult = IsAuth("Add lead", ContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				m_oServiceClient.Instance.BrokerAddCustomerLead(LeadFirstName, LeadLastName, LeadEmail, LeadAddMode, ContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to add lead for contact email {0}: {1} {2}, {3} - {4}.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);
				return new BrokerForJsonResult("Failed to add customer lead.");
			} // try

			ms_oLog.Debug("Broker add lead request for contact email {0}: {1} {2}, {3} - {4} complete.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);

			return new BrokerForJsonResult();
		} // AddLead

		#endregion action AddLead

		#region action SendInvitation

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SendInvitation(int nLeadID, string sContactEmail) {
			ms_oLog.Debug("Broker send invitation request for contact email {0} and lead id {1}.", sContactEmail, nLeadID);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Send invitation", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				m_oServiceClient.Instance.BrokerLeadSendInvitation(nLeadID, sContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to send invitation request for contact email {0} and lead id {1}.", sContactEmail, nLeadID);
				return new BrokerForJsonResult("Failed to send an invitation.");
			} // try

			ms_oLog.Debug("Broker send invitation request for contact email {0} and lead id {1} complete.", sContactEmail, nLeadID);

			return new BrokerForJsonResult();
		} // SendInvitation

		#endregion action SendInvitation

		#region action FillWizard

		[HttpGet]
		public System.Web.Mvc.ActionResult FillWizard(int? nLeadID, string sLeadEmail, string sContactEmail) {
			nLeadID = nLeadID ?? 0;

			ms_oLog.Debug("Broker fill wizard request for contact email {0} and lead id {1} lead email {2}.", sContactEmail, nLeadID, sLeadEmail);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Send invitation", sContactEmail);
			if (oIsAuthResult != null) {
				Session[Constant.Broker.MessageOnStart] = oIsAuthResult.error;
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // if

			if ((nLeadID > 0) && !string.IsNullOrWhiteSpace(sLeadEmail)) {
				ms_oLog.Warn("Both lead id ({0}) and lead email ({1}) specified while there can be only one.", nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // if

			BrokerLeadDetailsActionResult bld;

			try {
				bld = m_oServiceClient.Instance.BrokerLeadCanFillWizard(nLeadID.Value, sLeadEmail, sContactEmail);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to process fill wizard request for contact email {0} and lead id {1} lead email {2}.", sContactEmail, nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // try

			if (bld.LeadID < 1) {
				ms_oLog.Warn("Validated lead id is {0}. Source lead id is {1} lead email {2}.", bld.LeadID, nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // if

			m_oHelper.Logoff(User.Identity.Name, HttpContext);
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

			return RedirectToAction("Index", "Wizard", new { Area = "Customer" });
		} // FillWizard

		#endregion action FillWizard

		#region action FinishWizardLater

		[HttpGet]
		public System.Web.Mvc.ActionResult FinishWizardLater() {
			var blm = new WizardBrokerLeadModel(Session);

			ms_oLog.Debug("Broker fill wizard later request: {0}", blm);

			if (blm.BrokerFillsForCustomer) {
				StringActionResult sar = null;

				try {
					sar = m_oServiceClient.Instance.BrokerBackFromCustomerWizard(blm.LeadID);
				}
				catch (Exception e) {
					ms_oLog.Warn(e, "Failed to retrieve broker details, falling back to customer's dashboard.");
				} // try

				if (sar == null)
					ms_oLog.Debug("Failed to retrieve broker details.");
				else {
					BrokerHelper.SetAuth(null, HttpContext);

					ms_oLog.Debug("Restoring broker identity after filling customer wizard: '{0}'.", sar.Value);
					BrokerHelper.SetAuth(sar.Value);

					blm.Unset();
					return RedirectToAction("Index", "BrokerHome", new { Area = "Broker" });
				} // if
			} // if

			ms_oLog.Debug("Broker fill wizard later request failed, redirecting back to customer wizard.");

			blm.Unset();
			return RedirectToAction("Index", "Wizard", new { Area = "Customer" });
		} // FinishWizardLater

		#endregion action FinishWizardLater

		#region action DownloadFile

		public System.Web.Mvc.ActionResult DownloadFile(string fid) {
			if (string.IsNullOrWhiteSpace(fid)) {
				ms_oLog.Warn("Broker download file request: file with no id.");
				return HttpNotFound();
			} // if

			string sFileName = fid.Trim();

			var oFiles = new MarketingFiles(m_oServiceClient);

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

		#endregion action DownloadFile

		#region action UpdatePassword

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
				oResult = m_oServiceClient.Instance.BrokerUpdatePassword(
					ContactEmail,
					new Password(OldPassword),
					new Password(NewPassword, NewPassword2)
				);
			}
			catch (Exception e) {
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

		#endregion action UpdatePassword

		#region action SaveExperianDirector

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
				m_oServiceClient.Instance.UpdateExperianDirectorDetails(null, null, m);
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to save Experian director details.");
				return new BrokerForJsonResult("Failed to save director details.");
			} // try

			return new BrokerForJsonResult();
		} // SaveExperianDirector

		#endregion action SaveExperianDirector
	} // class BrokerHomeController
} // namespace
