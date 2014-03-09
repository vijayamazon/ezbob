﻿namespace EzBob.Web.Areas.Broker.Controllers {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
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
	using log4net;

	using Scorto.Web;
	using StructureMap;
	using ActionResult = EzServiceReference.ActionResult;

	#endregion using

	public class BrokerHomeController : Controller {
		#region public

		#region constructor

		public BrokerHomeController() {
			m_oConfig = ObjectFactory.GetInstance<IEzBobConfiguration>();
			m_oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region action Index (default)

		// GET: /Broker/BrokerHome/
		public System.Web.Mvc.ActionResult Index() {
			const string sAuth = "auth";
			const string sForbidden = "-";

			ViewData["Config"] = m_oConfig;
			ViewData[sAuth] = string.Empty;

			if (User.Identity.IsAuthenticated) {
				BoolActionResult bar = null;

				try {
					bar = m_oServiceClient.Instance.IsBroker(User.Identity.Name);
				}
				catch (Exception e) {
					m_oLog.Warn(e, "Failed to determine validity of broker email {0}", User.Identity.Name);
				} // try

				string sAuthenticationResult = ((bar != null) && bar.Value) ? User.Identity.Name : sForbidden;

				ViewData[sAuth] = sAuthenticationResult;

				m_oLog.Info("Broker page sent to browser with authentication result: {0}", sAuthenticationResult);
			} // if

			return View();
		} // Index

		#endregion action Index (default)

		#region action Signup

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
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
			string Password2
		) {
			m_oLog.Debug(
				"Broker signup request:" +
				"\n\tFirm name: {0}" +
				"\n\tFirm reg num: {1}" +
				"\n\tContact person name: {2}" +
				"\n\tContact person email: {3}" +
				"\n\tContact person mobile: {4}" +
				"\n\tMobile code: {5}" +
				"\n\tContact person other phone: {6}" +
				"\n\tEstimated monthly amount: {7}",
				FirmName,
				FirmRegNum,
				ContactName,
				ContactEmail,
				ContactMobile,
				MobileCode,
				ContactOtherPhone,
				EstimatedMonthlyClientAmount
			);

			if (User.Identity.IsAuthenticated) {
				m_oLog.Warn("Signup request with contact email {0}: already authorised as {1}.", ContactEmail, User.Identity.Name);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			try {
				m_oServiceClient.Instance.BrokerSignup(
					FirmName,
					FirmRegNum,
					ContactName,
					ContactEmail,
					ContactMobile,
					MobileCode,
					ContactOtherPhone,
					EstimatedMonthlyClientAmount,
					Password,
					Password2
				);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to signup as a broker.");
				return new BrokerForJsonResult("Failed to signup.");
			} // try

			FormsAuthentication.SetAuthCookie(ContactEmail, true);

			m_oLog.Debug("Broker signup succeded for: {0}", ContactEmail);

			return new BrokerForJsonResult();
		} // Signup

		#endregion action Signup

		#region action Logoff

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Logoff(string sContactEmail) {
			if (User.Identity.IsAuthenticated && (User.Identity.Name == sContactEmail)) {
				m_oLog.Debug("Broker {0} signed out.", User.Identity.Name);

				FormsAuthentication.SignOut();

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

			try {
				m_oServiceClient.Instance.BrokerLogin(LoginEmail, LoginPassword);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to login as a broker.");
				return new BrokerForJsonResult("Failed to log in.");
			} // try

			FormsAuthentication.SetAuthCookie(LoginEmail, true);

			m_oLog.Debug("Broker login succeded for: {0}", LoginEmail);

			return new BrokerForJsonResult();
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

		#region action LoadCustomers

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomers(string sContactEmail) {
			m_oLog.Debug("Broker load customers request for contact email {0}", sContactEmail);

			var oIsAuthResult = IsAuth<DataTablesBrokerForJsonResult>("Load customers", sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomersActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadCustomerList(sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load customers request for contact email {0}", sContactEmail);
				return new DataTablesBrokerForJsonResult("Failed to load customer list.");
			} // try

			m_oLog.Debug("Broker load customers request for contact email {0} complete.", sContactEmail);

			return new DataTablesBrokerForJsonResult(oData: oResult.Records);
		} // LoadCustomers

		#endregion action LoadCustomers

		#region action LoadCustomerDetails

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomerDetails(int nCustomerID, string sContactEmail) {
			m_oLog.Debug("Broker load customer details request for customer {1} and contact email {0}", sContactEmail, nCustomerID);

			var oIsAuthResult = IsAuth<CustomerDetailsBrokerForJsonResult>("Load customer details for customer " + nCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomerDetailsActionResult oDetails;

			try {
				oDetails = m_oServiceClient.Instance.BrokerLoadCustomerDetails(nCustomerID, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load customer details request for customer {1} and contact email {0}", sContactEmail, nCustomerID);
				return new CustomerDetailsBrokerForJsonResult("Failed to load customer details.");
			} // try

			m_oLog.Debug("Broker load customer details request for customer {1} and contact email {0} complete.", sContactEmail, nCustomerID);

			return new CustomerDetailsBrokerForJsonResult(oDetails: oDetails.Data);
		} // LoadCustomerDetails

		#endregion action LoadCustomerDetails

		#region action CrmLoadLookups

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult CrmLoadLookups() {
			m_oLog.Debug("Broker loading CRM details started...");

			CrmLookupsActionResult oLookups = null;

			try {
				oLookups = m_oServiceClient.Instance.CrmLoadLookups();
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Broker loading CRM details failed.");
				oLookups = new CrmLookupsActionResult();
			} // try

			m_oLog.Debug("Broker loading CRM details complete.");

			return new CrmLookupsBrokerForJsonResult(oLookups);
		} // CrmLoadLookups

		#endregion action CrmLoadLookups

		#region action SaveCrmEntry

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveCrmEntry(bool isIncoming, int action, int status, string comment, int customerId, string sContactEmail) {
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
		public JsonResult LoadCustomerFiles(int nCustomerID, string sContactEmail) {
			m_oLog.Debug("Broker load customer files request for customer {1} and contact email {0}", sContactEmail, nCustomerID);

			var oIsAuthResult = IsAuth<FileListBrokerForJsonResult>("Load customer files for customer " + nCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			BrokerCustomerFilesActionResult oFiles = null;

			try {
				oFiles = m_oServiceClient.Instance.BrokerLoadCustomerFiles(nCustomerID, sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load customer files request for customer {1} and contact email {0}", sContactEmail, nCustomerID);
				return new FileListBrokerForJsonResult("Failed to load customer files.");
			} // try

			m_oLog.Debug("Broker load customer files request for customer {1} and contact email {0} complete.", sContactEmail, nCustomerID);

			return new FileListBrokerForJsonResult(oFileList: oFiles.Files);
		} // LoadCustomerFiles

		#endregion action LoadCustomerFiles

		#region action HandleUploadFile

		[HttpPost]
		public JsonResult HandleUploadFile() {
			string sContactEmail = Request.Headers["ezbob-broker-contact-email"];

			int nCustomerID = 0;
			int.TryParse(Request.Headers["ezbob-broker-customer-id"], out nCustomerID);

			m_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0}", sContactEmail, nCustomerID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Upload customer file for customer " + nCustomerID, sContactEmail);
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
					m_oServiceClient.Instance.BrokerSaveUploadedCustomerFile(nCustomerID, sContactEmail, oFileContents, oFile.FileName);
				}
				catch (Exception e) {
					m_oLog.Alert(e, "Failed to save file #{0}: {2} out of {1}.", (i + 1), nFileCount, oFile.FileName);
					oErrorList.Add("Failed to save file #" + (i + 1) + ": " + oFile.FileName);
				} // try
			} // for each file

			m_oLog.Debug("Broker upload customer file request for customer {1} and contact email {0} complete.", sContactEmail, nCustomerID);

			return new BrokerForJsonResult(oErrorList.Count == 0 ? string.Empty : string.Join(" ", oErrorList));
		} // HandleUploadFile

		#endregion action HandleUploadFile

		#region action DownloadCustomerFile

		[HttpGet]
		public FileResult DownloadCustomerFile(int nCustomerID, string sContactEmail, int nFileID) {
			m_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2}", sContactEmail, nCustomerID, nFileID);

			BrokerForJsonResult oIsAuthResult = IsAuth("Download customer file for customer " + nCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				throw new Exception(oIsAuthResult.error);

			BrokerCustomerFileContentsActionResult oFile = null;

			try {
				oFile = m_oServiceClient.Instance.BrokerDownloadCustomerFile(nCustomerID, sContactEmail, nFileID);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to download customer file for customer {1} and contact email {0} with file id {2}", sContactEmail, nCustomerID, nFileID);
				throw new Exception("Failed to download requested file.");
			} // try

			if (string.IsNullOrWhiteSpace(oFile.Name)) {
				m_oLog.Alert("Could not download customer file for customer {1} and contact email {0} with file id {2}", sContactEmail, nCustomerID, nFileID);
				throw new Exception("Failed to download requested file.");
			} // if

			m_oLog.Debug("Broker download customer file request for customer {1} and contact email {0} with file id {2} complete.", sContactEmail, nCustomerID, nFileID);

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
		public JsonResult DeleteCustomerFiles(int nCustomerID, string sContactEmail, int[] aryFileIDs) {
			string sErrorMsg = null;

			if (aryFileIDs == null)
				sErrorMsg = "list of file ids is null after parsing";
			else if (aryFileIDs.Length < 1)
				sErrorMsg = "list of file ids is empty after parsing";

			if (!string.IsNullOrWhiteSpace(sErrorMsg)) {
				m_oLog.Alert("Failed to delete customer files request for customer {1} and contact email {0}: {2}.", sContactEmail, nCustomerID, sErrorMsg);
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // if

			m_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, nCustomerID, string.Join(", ", aryFileIDs));

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Delete customer files for customer " + nCustomerID, sContactEmail);
			if (oIsAuthResult != null)
				return oIsAuthResult;

			try {
				m_oServiceClient.Instance.BrokerDeleteCustomerFiles(nCustomerID, sContactEmail, aryFileIDs);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to delete customer files request for customer {1} and contact email {0}; file ids: {2}", sContactEmail, nCustomerID, string.Join(", ", aryFileIDs));
				return new BrokerForJsonResult("Failed to delete customer files.");
			} // try

			m_oLog.Debug("Broker delete customer files request for customer {1} and contact email {0}; file ids: {2} complete.", sContactEmail, nCustomerID, string.Join(", ", aryFileIDs));

			return new BrokerForJsonResult();
		} // DeleteCustomerFiles

		#endregion action DeleteCustomerFiles

		#endregion public

		#region private

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

				return (T)typeof(T).GetConstructors().FirstOrDefault().Invoke(new object[] { "Not authorised." });
			} // if

			return null;
		} // IsAuth

		#endregion method IsAuth

		#region fields

		private readonly IEzBobConfiguration m_oConfig;
		private readonly ASafeLog m_oLog;
		private readonly ServiceClient m_oServiceClient;

		#endregion fields

		#region result classes
// ReSharper disable InconsistentNaming

		#region class BrokerForJsonResult

		public class BrokerForJsonResult {
			#region operator cast to JsonResult

			public static implicit operator JsonResult(BrokerForJsonResult oResult) {
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
				private set { m_sError = (value ?? string.Empty).Trim(); }
			} // error

			private string m_sError;

			#endregion proprety error
		} // BrokerForJsonResult

		#endregion class BrokerForJsonResult

		#region class DataTablesBrokerForJsonResult

		public class DataTablesBrokerForJsonResult : BrokerForJsonResult {
			public DataTablesBrokerForJsonResult(string sErrorMsg = "", bool? bExplicitSuccess = null, BrokerCustomerEntry[] oData = null) : base(sErrorMsg, bExplicitSuccess) {
				aaData = oData ?? new BrokerCustomerEntry[0];
			} // constructor

			public virtual BrokerCustomerEntry[] aaData { get; private set; } // aaData
		} // DataTablesBrokerForJsonResult

		#endregion class DataTablesBrokerForJsonResult

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

		#region class CrmLookupsBrokerForJsonResult

		public class CrmLookupsBrokerForJsonResult : BrokerForJsonResult {
			public CrmLookupsBrokerForJsonResult(
				CrmLookupsActionResult oLookups,
				string sErrorMsg = "",
				bool? bExplicitSuccess = null
			) : base(sErrorMsg, bExplicitSuccess) {
				actions = oLookups.Actions.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
				statuses = oLookups.Statuses.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
			} // constructor

			public virtual Dictionary<string, string> actions { get; private set; } // actions

			public virtual Dictionary<string, string> statuses { get; private set; } // statuses
		} // CrmLookupsBrokerForJsonResult

		#endregion class CrmLookupsBrokerForJsonResult

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

		#endregion private
	} // class BrokerHomeController
} // namespace
