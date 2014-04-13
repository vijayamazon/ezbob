﻿namespace EzBob.Web.Areas.Broker.Controllers {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;

	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.csrf;
	using EzServiceReference;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;

	using Ezbob.Utils;
	using Infrastructure.Attributes;
	using Infrastructure.Filters;
	using Models;
	using log4net;

	#endregion using

	public class BrokerHomeController : Controller {
		#region static constructor

		static BrokerHomeController() {
			m_oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));
		} // static constructor

		#endregion static constructor

		#region public

		#region constructor

		public BrokerHomeController() {
			m_oServiceClient = new ServiceClient();
			m_oHelper = new BrokerHelper(m_oServiceClient, m_oLog);
		} // constructor

		#endregion constructor

		#region action Index (default)

		// GET: /Broker/BrokerHome/
		public System.Web.Mvc.ViewResult Index(string sourceref = "") {
			MarketingFiles oFiles = LoadMarketingFiles(false);

			var oModel = new BrokerHomeModel(oFiles == null ? null : oFiles.Ordinal);

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

				m_oLog.Info("Broker page sent to browser with authentication result '{0}' for identified name '{1}'.", oModel.Auth, User.Identity.Name);
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
			string sReferredBy = Request.Cookies.AllKeys.Contains(Constant.SourceRef) ? Request.Cookies[Constant.SourceRef].Value : "";

			m_oLog.Debug(
				"Broker signup request:" +
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
				m_oLog.Warn("Signup request with contact email {0}: already authorised as {1}.", ContactEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerPropertiesActionResult bp = null;

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
					Password,
					Password2,
					FirmWebSite,
					EstimatedMonthlyAppCount,
					IsCaptchaEnabled != 0,
					TermsID,
					sReferredBy
				);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to signup as a broker.");
				return new BrokerForJsonResult("Registration failed. Please contact customer care.");
			} // try

			FormsAuthentication.SetAuthCookie(ContactEmail, true);

			m_oLog.Debug("Broker signup succeded for: {0}", ContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp.Properties);
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
				return new BrokerForJsonResult();
			} // if

			m_oLog.Warn(
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
			m_oLog.Debug("Broker login request: {0}", LoginEmail);

			if (User.Identity.IsAuthenticated) {
				m_oLog.Warn("Login request with contact email {0}: already authorised as {1}.", LoginEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerProperties bp = m_oHelper.TryLogin(LoginEmail, LoginPassword);

			if (bp == null)
				return new BrokerForJsonResult("Failed to log in.");

			m_oLog.Debug("Broker login succeded for: {0}", LoginEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp);
		} // Login

		#endregion action Login

		#region action RestorePassword

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RestorePassword(string ForgottenMobile, string ForgottenMobileCode) {
			m_oLog.Debug("Broker restore password request: phone # {0} with code {1}", ForgottenMobile, ForgottenMobileCode);

			if (User.Identity.IsAuthenticated) {
				m_oLog.Warn("Request with mobile phone {0} and code {1}: already authorised as {2}.", ForgottenMobile, ForgottenMobileCode, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			try {
				m_oServiceClient.Instance.BrokerRestorePassword(ForgottenMobile, ForgottenMobileCode);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to restore password for a broker with phone # {0}.", ForgottenMobile);
				return new BrokerForJsonResult("Failed to restore password.");
			} // try

			m_oLog.Debug("Broker restore password succeded for phone # {0}", ForgottenMobile);

			return new BrokerForJsonResult();
		} // RestorePassword

		#endregion action RestorePassword

		#region action LoadProperties

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadProperties(string sContactEmail) {
			m_oLog.Debug("Broker load properties request for contact email {0}", sContactEmail);

			var oIsAuthResult = IsAuth<PropertiesBrokerForJsonResult>("Load properties", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerPropertiesActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadOwnProperties(sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load properties request for contact email {0}", sContactEmail);
				return new PropertiesBrokerForJsonResult("Failed to load broker properties.");
			} // try

			m_oLog.Debug("Broker load customers properties for contact email {0} complete.", sContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: oResult.Properties);
		} // LoadProperties

		#endregion action LoadProperties

		#region action LoadCustomers

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomers(string sContactEmail) {
			m_oLog.Debug("Broker load customers request for contact email {0}", sContactEmail);

			var oIsAuthResult = IsAuth<CustomerListBrokerForJsonResult>("Load customers", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomersActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadCustomerList(sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load customers request for contact email {0}", sContactEmail);
				return new CustomerListBrokerForJsonResult("Failed to load customer list.");
			} // try

			m_oLog.Debug("Broker load customers request for contact email {0} complete.", sContactEmail);

			return new CustomerListBrokerForJsonResult(oCustomers: oResult.Customers);
		} // LoadCustomers

		#endregion action LoadCustomers

		#region action LoadCustomerDetails

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomerDetails(string sCustomerID, string sContactEmail) {
			m_oLog.Debug("Broker load customer details request for customer {1} and contact email {0}", sContactEmail, sCustomerID);

			var oIsAuthResult = IsAuth<CustomerDetailsBrokerForJsonResult>("Load customer details for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomerDetailsActionResult oDetails;

			try {
				oDetails = m_oServiceClient.Instance.BrokerLoadCustomerDetails(sCustomerID, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load customer details request for customer {1} and contact email {0}", sContactEmail, sCustomerID);
				return new CustomerDetailsBrokerForJsonResult("Failed to load customer details.");
			} // try

			m_oLog.Debug("Broker load customer details request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new CustomerDetailsBrokerForJsonResult(oDetails: oDetails.Data);
		} // LoadCustomerDetails

		#endregion action LoadCustomerDetails

		#region action LoadStaticData

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadStaticData() {
			m_oLog.Debug("Broker loading CRM details started...");

			BrokerStaticDataActionResult oResult = null;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadStaticData();
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Broker loading static data failed.");

				oResult = new BrokerStaticDataActionResult {
					MaxPerNumber = 3,
					MaxPerPage = 10,
					Files = new FileDescription[0],
					Actions = new Dictionary<int, string>(),
					Statuses = new Dictionary<int, string>(),
					Terms = "",
					TermsID = 0,
				};
			} // try

			m_oLog.Debug("Broker loading CRM details complete.");

			return new StaticDataBrokerForJsonResult(oResult);
		} // LoadStaticData

		#endregion action LoadStaticData

		#region action SaveCrmEntry

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveCrmEntry(bool isIncoming, int action, int status, string comment, string customerId, string sContactEmail) {
			m_oLog.Debug(
				"\nBroker saving CRM entry started:" +
				"\n\tis incoming: {0}" +
				"\n\taction: {1}" +
				"\n\tstatus: {2}" +
				"\n\tcustomer id: {3}" +
				"\n\tcontact email: {4}" +
				"\n\tcomment: {5}\n",
				isIncoming, action, status, customerId, sContactEmail, comment
			);

			BrokerForJsonResult oIsAuthResult = IsAuth("Save CRM entry for customer " + customerId, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			StringActionResult oResult = null;

			try {
				oResult = m_oServiceClient.Instance.BrokerSaveCrmEntry(isIncoming, action, status, comment, customerId, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e,
					"\nBroker saving CRM entry failed for:" +
					"\n\tis incoming: {0}" +
					"\n\taction: {1}" +
					"\n\tstatus: {2}" +
					"\n\tcustomer id: {3}" +
					"\n\tcontact email: {4}",
					isIncoming, action, status, customerId, sContactEmail
				);

				return new BrokerForJsonResult("Failed to save CRM entry.");
			} // try

			m_oLog.Debug(
				"\nBroker saving CRM entry {5} for:" +
				"\n\tis incoming: {0}" +
				"\n\taction: {1}" +
				"\n\tstatus: {2}" +
				"\n\tcustomer id: {3}" +
				"\n\tcontact email: {4}\n" +
				"\n\terror message: {6}\n",
				isIncoming, action, status, customerId, sContactEmail,
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
			m_oLog.Debug("Broker load customer files request for customer {1} and contact email {0}", sContactEmail, sCustomerID);

			var oIsAuthResult = IsAuth<FileListBrokerForJsonResult>("Load customer files for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomerFilesActionResult oFiles = null;

			try {
				oFiles = m_oServiceClient.Instance.BrokerLoadCustomerFiles(sCustomerID, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load customer files request for customer {1} and contact email {0}", sContactEmail, sCustomerID);
				return new FileListBrokerForJsonResult("Failed to load customer files.");
			} // try

			m_oLog.Debug("Broker load customer files request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new FileListBrokerForJsonResult(oFileList: oFiles.Files);
		} // LoadCustomerFiles

		#endregion action LoadCustomerFiles

		#region action HandleUploadFile

		[HttpPost]
		public JsonResult HandleUploadFile() {
			string sContactEmail = Request.Headers["ezbob-broker-contact-email"];

			string sCustomerID = Request.Headers["ezbob-broker-customer-id"];

			m_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0}", sContactEmail, sCustomerID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Upload customer file for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			var nFileCount = Request.Files.Count;

			var oErrorList = new List<string>();

			for (int i = 0; i < nFileCount; i++) {
				HttpPostedFileBase oFile = Request.Files[i];

				if (oFile == null) {
					m_oLog.Alert("File object #{0} out of {1} is null.", (i + 1), nFileCount);
					oErrorList.Add("Failed to upload file #" + (i + 1));
					continue;
				} // if

				var oFileContents = new byte[oFile.InputStream.Length];

				int nRead = oFile.InputStream.Read(oFileContents, 0, oFile.ContentLength);

				if (nRead != oFile.ContentLength) {
					oErrorList.Add("Failed to fully file #" + (i + 1) + ": " + oFile.FileName);
					m_oLog.Alert(
						"Failed to fully read file #{0}: {2} out of {1}; only {3} bytes out of {4} have been read.",
						(i + 1), nFileCount, oFile.FileName, nRead, oFile.ContentLength
					);
					continue;
				} // if

				m_oLog.Debug(
					"File #{0}: {2} out of {1}; file size is {3} bytes.",
					(i + 1), nFileCount, oFile.FileName, nRead
				);

				try {
					m_oServiceClient.Instance.BrokerSaveUploadedCustomerFile(sCustomerID, sContactEmail, oFileContents, oFile.FileName);
				}
				catch (Exception e) {
					m_oLog.Alert(e, "Failed to save file #{0}: {2} out of {1}.", (i + 1), nFileCount, oFile.FileName);
					oErrorList.Add("Failed to save file #" + (i + 1) + ": " + oFile.FileName);
				} // try
			} // for each file

			m_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0} complete.", sContactEmail, sCustomerID);

			return new BrokerForJsonResult(oErrorList.Count == 0 ? string.Empty : string.Join(" ", oErrorList));
		} // HandleUploadFile

		#endregion action HandleUploadFile

		#region action DownloadCustomerFile

		[HttpGet]
		public FileResult DownloadCustomerFile(string sCustomerID, string sContactEmail, int nFileID) {
			m_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Download customer file for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				throw new Exception(oIsAuthResult.error);

			BrokerCustomerFileContentsActionResult oFile = null;

			try {
				oFile = m_oServiceClient.Instance.BrokerDownloadCustomerFile(sCustomerID, sContactEmail, nFileID);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to download customer file for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);
				throw new Exception("Failed to download requested file.");
			} // try

			if (string.IsNullOrWhiteSpace(oFile.Name)) {
				m_oLog.Alert("Could not download customer file for customer {1} and contact email {0} with file id {2}", sContactEmail, sCustomerID, nFileID);
				throw new Exception("Failed to download requested file.");
			} // if

			m_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2} complete.", sContactEmail, sCustomerID, nFileID);

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

			if (aryFileIDs == null)
				sErrorMsg = "list of file ids is null after parsing";
			else if (aryFileIDs.Length < 1)
				sErrorMsg = "list of file ids is empty after parsing";

			if (!string.IsNullOrWhiteSpace(sErrorMsg)) {
				m_oLog.Alert("Failed to delete customer files request for customer {1} and contact email {0}: {2}.", sContactEmail, sCustomerID, sErrorMsg);
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // if

			m_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, sCustomerID, string.Join(", ", aryFileIDs));

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Delete customer files for customer " + sCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				m_oServiceClient.Instance.BrokerDeleteCustomerFiles(sCustomerID, sContactEmail, aryFileIDs);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, sCustomerID, string.Join(", ", aryFileIDs));
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // try

			m_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2} complete.", sContactEmail, sCustomerID, string.Join(", ", aryFileIDs));

			return new BrokerForJsonResult();
		} // DeleteCustomerFiles

		#endregion action DeleteCustomerFiles

		#region action AddLead

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddLead(string LeadFirstName, string LeadLastName, string LeadEmail, string LeadAddMode, string ContactEmail) {
			m_oLog.Debug("Broker add lead request for contact email {0}: {1} {2}, {3} - {4}.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);

			var oIsAuthResult = IsAuth("Add lead", ContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				m_oServiceClient.Instance.BrokerAddCustomerLead(LeadFirstName, LeadLastName, LeadEmail, LeadAddMode, ContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to add lead for contact email {0}: {1} {2}, {3} - {4}.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);
				return new BrokerForJsonResult("Failed to add customer lead.");
			} // try

			m_oLog.Debug("Broker add lead request for contact email {0}: {1} {2}, {3} - {4} complete.", ContactEmail, LeadFirstName, LeadLastName, LeadEmail, LeadAddMode);

			return new BrokerForJsonResult();
		} // AddLead

		#endregion action AddLead

		#region action SendInvitation

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SendInvitation(int nLeadID, string sContactEmail) {
			m_oLog.Debug("Broker send invitation request for contact email {0} and lead id {1}.", sContactEmail, nLeadID);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Send invitation", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				m_oServiceClient.Instance.BrokerLeadSendInvitation(nLeadID, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to send invitation request for contact email {0} and lead id {1}.", sContactEmail, nLeadID);
				return new BrokerForJsonResult("Failed to send an invitation.");
			} // try

			m_oLog.Debug("Broker send invitation request for contact email {0} and lead id {1} complete.", sContactEmail, nLeadID);

			return new BrokerForJsonResult();
		} // SendInvitation

		#endregion action SendInvitation

		#region action FillWizard

		[HttpGet]
		public System.Web.Mvc.ActionResult FillWizard(int? nLeadID, string sLeadEmail, string sContactEmail) {
			nLeadID = nLeadID ?? 0;

			m_oLog.Debug("Broker fill wizard request for contact email {0} and lead id {1} lead email {2}.", sContactEmail, nLeadID, sLeadEmail);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Send invitation", sContactEmail);
			if (oIsAuthResult != null) {
				Session[Constant.Broker.MessageOnStart] = oIsAuthResult.error;
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // if

			if ((nLeadID > 0) && !string.IsNullOrWhiteSpace(sLeadEmail)) {
				m_oLog.Warn("Both lead id ({0}) and lead email ({1}) specified while there can be only one.", nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // if

			BrokerLeadDetailsActionResult bld = null;

			try {
				bld = m_oServiceClient.Instance.BrokerLeadCanFillWizard(nLeadID.Value, sLeadEmail, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to process fill wizard request for contact email {0} and lead id {1} lead email {2}.", sContactEmail, nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // try

			if (bld.LeadID < 1) {
				m_oLog.Warn("Validated lead id is {0}. Source lead id is {1} lead email {2}.", bld.LeadID, nLeadID, sLeadEmail);

				Session[Constant.Broker.MessageOnStart] = "Could not process fill all the details request.";
				Session[Constant.Broker.MessageOnStartSeverity] = Constant.Severity.Error;

				return RedirectToAction("Index", "BrokerHome", new { Area = "Broker", });
			} // if

			m_oHelper.Logoff(User.Identity.Name, HttpContext);
			if (bld.CustomerID > 0)
				FormsAuthentication.SetAuthCookie(bld.LeadEmail, false);

			// This constructor sets Session data.
			new WizardBrokerLeadModel(
				Session,
				bld.LeadID,
				bld.LeadEmail, 
				bld.FirstName,
				bld.LastName,
				true
			);

			m_oLog.Debug("Broker fill wizard request for contact email {0} and lead id {1} lead email {2} complete.", sContactEmail, nLeadID, sLeadEmail);

			return RedirectToAction("Index", "Wizard", new { Area = "Customer" });
		} // FillWizard

		#endregion action FillWizard

		#region action FinishWizardLater

		[HttpGet]
		public System.Web.Mvc.ActionResult FinishWizardLater() {
			var blm = new WizardBrokerLeadModel(Session);

			m_oLog.Debug("Broker fill wizard later request: {0}", blm);

			if (blm.BrokerFillsForCustomer) {
				StringActionResult sar = null;

				try {
					sar = m_oServiceClient.Instance.BrokerBackFromCustomerWizard(blm.LeadID);
				}
				catch (Exception e) {
					m_oLog.Warn("Failed to retrieve broker details, falling back to customer's dashboard.", e);
				} // try

				if (sar == null)
					m_oLog.Debug("Failed to retrieve broker details.");
				else {
					FormsAuthentication.SignOut();
					HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

					m_oLog.Debug("Restoring broker identity after filling customer wizard: '{0}'.", sar.Value);
					FormsAuthentication.SetAuthCookie(sar.Value, true);

					blm.Unset();
					return RedirectToAction("Index", "BrokerHome", new {Area = "Broker"});
				} // if
			} // if

			m_oLog.Debug("Broker fill wizard later request failed, redirecting back to customer wizard.");

			blm.Unset();
			return RedirectToAction("Index", "Wizard", new { Area = "Customer" });
		} // FinishWizardLater

		#endregion action FinishWizardLater

		#region action DownloadFile

		public System.Web.Mvc.ActionResult DownloadFile(string fid) {
			if (string.IsNullOrWhiteSpace(fid)) {
				m_oLog.Warn("Broker download file request: file with no id.");
				return HttpNotFound();
			} // if

			string sFileName = fid.Trim();

			MarketingFiles oFiles = LoadMarketingFiles(true);

			m_oLog.Debug("Broker download file request: file with id {0}.", sFileName);

			FileDescription fd = oFiles.Find(sFileName);

			if (fd != null) {
				string sPath = System.Web.HttpContext.Current.Server.MapPath("~/Areas/Broker/Files/" + fd.FileName);

				m_oLog.Debug("Broker download file request: found file with id {0} of type {1} as {2}.", sFileName, fd.MimeType, sPath);

				return File(sPath, fd.MimeType, fd.FileName);
			} // if

			m_oLog.Debug("Broker download file request: file with id {0} was not found.", sFileName);

			return HttpNotFound();
		} // DownloadFile

		#endregion action DownloadFile

		#region action UpdatePassword

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UpdatePassword(string ContactEmail, string OldPassword, string NewPassword, string NewPassword2) {
			m_oLog.Debug("Broker update password request for contact email {0}", ContactEmail);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Update password", ContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			if (ReferenceEquals(OldPassword, null) || ReferenceEquals(NewPassword, null) || ReferenceEquals(NewPassword2, null)) {
				m_oLog.Warn("Cannot update password for contact email {0}: one of passwords not specified.", ContactEmail);
				return new BrokerForJsonResult("Cannot update password: some required fields are missing.");
			} // if

			if (NewPassword != NewPassword2) {
				m_oLog.Warn("Cannot update password: passwords do not match.");
				return new BrokerForJsonResult("Cannot update password: passwords do not match.");
			} // if

			if (NewPassword == OldPassword) {
				m_oLog.Warn("Cannot update password: new password is equal to the old one.");
				return new BrokerForJsonResult("Cannot update password: new password is equal to the old one.");
			} // if

			ActionMetaData oResult = null;

			try {
				oResult = m_oServiceClient.Instance.BrokerUpdatePassword(ContactEmail, OldPassword, NewPassword, NewPassword2);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to update password for contact email {0}", ContactEmail);
				return new BrokerForJsonResult("Failed to update password.");
			} // try

			if (oResult == null) {
				m_oLog.Warn("Failed to update password for contact email {0}", ContactEmail);
				return new BrokerForJsonResult("Failed to update password.");
			} // if

			m_oLog.Debug("Broker update password request for contact email {0} complete.", ContactEmail);

			return new BrokerForJsonResult();
		} // UpdatePassword

		#endregion action UpdatePassword

		#endregion public

		#region private

		#region method LoadMarketingFiles

		private MarketingFiles LoadMarketingFiles(bool bCreateAlphabetical) {
			m_oLog.Debug("Loading broker marketing files...");

			BrokerStaticDataActionResult flar = null;

			try {
				flar = m_oServiceClient.Instance.BrokerLoadStaticData(); // TODO: add argument to retrieve files only
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load broker marketing files.");
			} // try

			var oResult = new MarketingFiles(flar == null ? null : flar.Files, bCreateAlphabetical);

			m_oLog.Debug("Loading broker marketing files complete.");

			return oResult;
		} // LoadMarketingFiles

		#endregion method LoadMarketingFiles

		#region method IsAuth

		private BrokerForJsonResult IsAuth(string sRequestDescription, string sContactEmail) {
			return IsAuth<BrokerForJsonResult>(sRequestDescription, sContactEmail);
		} // IsAuth

		private T IsAuth<T>(string sRequestDescription, string sContactEmail) where T: BrokerForJsonResult {
			if (!User.Identity.IsAuthenticated || (User.Identity.Name != sContactEmail)) {
				m_oLog.Alert(
					"{0} request with contact email {1}: {2}.",
					sRequestDescription,
					sContactEmail,
					User.Identity.IsAuthenticated ? "authorised as " + User.Identity.Name : "not authenticated"
				);

				ConstructorInfo ci = typeof (T).GetConstructors().FirstOrDefault();

				int nParamCount = ci.GetParameters().Length;

				var oConstructorArgs = new object[nParamCount];

				for (int i = 0; i < nParamCount; i++)
					oConstructorArgs[i] = null;

				T oResult = (T)ci.Invoke(oConstructorArgs);

				oResult.error = "Not authorised.";
				oResult.is_auth = false;

				return oResult;
			} // if

			return null;
		} // IsAuth

		#endregion method IsAuth

		#region fields

		private readonly ServiceClient m_oServiceClient;
		private readonly BrokerHelper m_oHelper;

		private static readonly ASafeLog m_oLog;

		#endregion fields

		#region result classes
// ReSharper disable InconsistentNaming

		#region class BrokerForJsonResult

		public class BrokerForJsonResult {
			#region operator cast to JsonResult

			public static implicit operator JsonResult(BrokerForJsonResult oResult) {
				BrokerHomeController.m_oLog.Debug(
					"Controller output:\n\ttype: {0}\n\terror msg: {1}",
					oResult.GetType(), oResult.error
				);

				return new JsonResult {
					Data = oResult, 
					ContentType = null,
					ContentEncoding = null, 
					JsonRequestBehavior = JsonRequestBehavior.AllowGet,
				};
			} // operator JsonResult

			#endregion operator cast to JsonResult

			#region constructor

			public BrokerForJsonResult(string sErrorMsg = "", bool? bExplicitSuccess = null) {
				is_auth = true;
				error = sErrorMsg;
				m_bSuccess = bExplicitSuccess;
			} // constructor

			#endregion constructor

			#region proprety success

			public virtual bool success {
				get { return m_bSuccess.HasValue ? m_bSuccess.Value : string.IsNullOrWhiteSpace(error); } 
			} // success

			private bool? m_bSuccess;

			#endregion proprety success

			#region proprety error

			public virtual string error {
				get { return m_sError; }
				set { m_sError = (value ?? string.Empty).Trim(); }
			} // error

			private string m_sError;

			#endregion proprety error

			#region proprety is_auth

			public virtual bool is_auth { get; set; } // is_auth

			#endregion proprety is_auth
		} // BrokerForJsonResult

		#endregion class BrokerForJsonResult

		#region class PropertiesBrokerForJsonResult

		public class PropertiesBrokerForJsonResult : BrokerForJsonResult {
			public PropertiesBrokerForJsonResult(
				string sErrorMsg = "",
				bool? bExplicitSuccess = null,
				BrokerProperties oProperties = null
			) : base(sErrorMsg, bExplicitSuccess) {
				properties = oProperties ?? new BrokerProperties();
			} // constructor

			public virtual BrokerProperties properties { get; private set; } // customers
		} // PropertiesBrokerForJsonResult

		#endregion class PropertiesBrokerForJsonResult

		#region class CustomerListBrokerForJsonResult

		public class CustomerListBrokerForJsonResult : BrokerForJsonResult {
			public CustomerListBrokerForJsonResult(
				string sErrorMsg = "",
				bool? bExplicitSuccess = null,
				BrokerCustomerEntry[] oCustomers = null
			) : base(sErrorMsg, bExplicitSuccess) {
				customers = oCustomers ?? new BrokerCustomerEntry[0];
			} // constructor

			public virtual BrokerCustomerEntry[] customers { get; private set; } // customers
		} // CustomerListBrokerForJsonResult

		#endregion class CustomerListBrokerForJsonResult

		#region class CustomerDetailsBrokerForJsonResult

		public class CustomerDetailsBrokerForJsonResult : BrokerForJsonResult {
			public CustomerDetailsBrokerForJsonResult(string sErrorMsg = "", bool? bExplicitSuccess = null, BrokerCustomerDetails oDetails = null) : base(sErrorMsg, bExplicitSuccess) {
				crm_data = oDetails == null ? null : oDetails.CrmData;
				personal_data = oDetails == null ? null : oDetails.PersonalData;
			} // constructor

			public virtual List<BrokerCustomerCrmEntry> crm_data { get; private set; }

			public virtual BrokerCustomerPersonalData personal_data { get; private set; }
		} // CustomerDetailsBrokerForJsonResult

		#endregion class CustomerDetailsBrokerForJsonResult

		#region class StaticDataBrokerForJsonResult

		public class StaticDataBrokerForJsonResult : BrokerForJsonResult {
			public StaticDataBrokerForJsonResult(
				BrokerStaticDataActionResult oResult,
				string sErrorMsg = "",
				bool? bExplicitSuccess = null
			) : base(sErrorMsg, bExplicitSuccess) {
				actions = oResult.Actions.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);

				statuses = oResult.Statuses.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);

				broker_terms = new Dictionary<string, string>() {
					{ "id", oResult.TermsID.ToString() },
					{ "text", oResult.Terms },
				};

				max_per_number = oResult.MaxPerNumber;
				max_per_page = oResult.MaxPerPage;
			} // constructor

			public virtual int max_per_number { get; private set; }

			public virtual int max_per_page { get; private set; }

			public virtual Dictionary<string, string> actions { get; private set; } // actions

			public virtual Dictionary<string, string> statuses { get; private set; } // statuses

			public virtual Dictionary<string, string> broker_terms { get; private set; } // broker_terms
		} // StaticDataBrokerForJsonResult

		#endregion class StaticDataBrokerForJsonResult

		#region class FileListBrokerForJsonResult

		public class FileListBrokerForJsonResult : BrokerForJsonResult {
			public FileListBrokerForJsonResult(string sErrorMsg = "", bool? bExplicitSuccess = null, BrokerCustomerFile[] oFileList = null) : base(sErrorMsg, bExplicitSuccess) {
				file_list = oFileList ?? new BrokerCustomerFile[0];
			} // constructor

			public virtual BrokerCustomerFile[] file_list { get; private set; } // file_list
		} // FileListBrokerForJsonResult

		#endregion class FileListBrokerForJsonResult

// ReSharper restore InconsistentNaming
		#endregion result classes

		#region downloadable file descriptor

		private class MarketingFiles {
			public MarketingFiles(FileDescription[] oOrdinalList, bool bCreateAlphabetical) {
				Ordinal = oOrdinalList ?? new FileDescription[0];

				if (!bCreateAlphabetical)
					return;

				m_oAlphabetical = new SortedDictionary<string, FileDescription>();

				foreach (FileDescription fd in Ordinal)
					m_oAlphabetical[fd.FileID] = fd;
			} // constructor

			public FileDescription[] Ordinal { get; private set; }

			public FileDescription Find(string sKey) {
				if (m_oAlphabetical == null)
					return null;

				return m_oAlphabetical.ContainsKey(sKey) ? m_oAlphabetical[sKey] : null;
			} // Find

			private readonly SortedDictionary<string, FileDescription> m_oAlphabetical;
		} // class MarketingFiles

		#endregion downloadable file descriptor

		#endregion private
	} // class BrokerHomeController
} // namespace
