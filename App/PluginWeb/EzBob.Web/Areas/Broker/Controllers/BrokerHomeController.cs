﻿namespace EzBob.Web.Areas.Broker.Controllers {
	using System;
	using System.Web.Mvc;
	using System.Web.Security;

	using EzBob.Web.Code.ApplicationCreator;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.csrf;

	using Ezbob.Backend.Models;
	using Ezbob.Logger;

	using EzServiceReference;

	using log4net;
	using Scorto.Web;

	public class BrokerHomeController : Controller {
		#region public

		#region constructor

		public BrokerHomeController(
			IAppCreator oAppCreator,
			IEzBobConfiguration config
		) {
			m_oAppCreator = oAppCreator;
			m_oConfig = config;
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
					bar = m_oAppCreator.ServiceClient.IsBroker(User.Identity.Name);
				}
				catch (Exception e) {
					ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));
					oLog.Warn(e, "Failed to determine validity of broker email {0}", User.Identity.Name);
				} // try

				ViewData[sAuth] = ((bar != null) && bar.Value) ? User.Identity.Name : sForbidden;
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
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));

			oLog.Debug(
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

			try {
				m_oAppCreator.ServiceClient.BrokerSignup(
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
				oLog.Alert(e, "Failed to signup as a broker.");
				return Json(new { success = false, error = "Failed to signup.", });
			} // try

			FormsAuthentication.SetAuthCookie(ContactEmail, true);

			oLog.Debug("Broker signup succeded for: {0}", ContactEmail);

			return Json(new { success = true, error = string.Empty, });
		} // Signup

		#endregion action Signup

		#region action Logoff

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Logoff() {
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));

			if (User.Identity.IsAuthenticated)
				oLog.Debug("Broker {0} signed out.", User.Identity.Name);

			FormsAuthentication.SignOut();

			return Json(new { success = true, error = string.Empty, });
		} // Logoff

		#endregion action Logoff

		#region action Login

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Login(string LoginEmail, string LoginPassword) {
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));

			oLog.Debug("Broker login request: {0}", LoginEmail);

			try {
				m_oAppCreator.ServiceClient.BrokerLogin(LoginEmail, LoginPassword);
			}
			catch (Exception e) {
				oLog.Alert(e, "Failed to login as a broker.");
				return Json(new { success = false, error = "Failed to login.", });
			} // try

			FormsAuthentication.SetAuthCookie(LoginEmail, true);

			oLog.Debug("Broker login succeded for: {0}", LoginEmail);

			return Json(new { success = true, error = string.Empty, });
		} // Login

		#endregion action Login

		#region action RestorePassword

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RestorePassword(string ForgottenMobile, string ForgottenMobileCode) {
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));

			oLog.Debug("Broker restore password request: phone # {0} with code {1}", ForgottenMobile, ForgottenMobileCode);

			try {
				m_oAppCreator.ServiceClient.BrokerRestorePassword(ForgottenMobile, ForgottenMobileCode);
			}
			catch (Exception e) {
				oLog.Alert(e, "Failed to restore password for a broker with phone # {0}.", ForgottenMobile);
				return Json(new { success = false, error = "Failed to restore password.", });
			} // try

			oLog.Debug("Broker restore password succeded for phone # {0}", ForgottenMobile);

			return Json(new { success = true, error = string.Empty, });
		} // RestorePassword

		#endregion action RestorePassword

		#region action LoadCustomers

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomers(string sContactEmail) {
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));

			oLog.Debug("Broker load customers request for contact email {0}", sContactEmail);

			if (!User.Identity.IsAuthenticated || (User.Identity.Name != sContactEmail)) {
				oLog.Debug("Failed to load customers request for contact email {0}: not authenticated or authenticated as other user.", sContactEmail);
				return Json(new { success = false, error = "Not authorised.", aaData = (BrokerCustomerEntry [])null }, JsonRequestBehavior.AllowGet);
			} // if

			BrokerCustomersActionResult oResult;

			try
			{
				oResult = m_oAppCreator.ServiceClient.BrokerLoadCustomerList(sContactEmail);
			}
			catch (Exception e) {
				oLog.Debug(e, "Failed to load customers request for contact email {0}", sContactEmail);
				return Json(new { success = false, error = "Failed to load customer list.", aaData = new BrokerCustomerEntry [] {} }, JsonRequestBehavior.AllowGet);
			} // try

			oLog.Debug("Broker load customers request for contact email {0} complete.", sContactEmail);

			return Json(new { success = true, error = string.Empty, aaData = oResult.Records }, JsonRequestBehavior.AllowGet);
		} // LoadCustomers

		#endregion action LoadCustomers

		#region action LoadCustomerDetails

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadCustomerDetails(int nCustomerID, string sContactEmail) {
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof(BrokerHomeController)));

			oLog.Debug("Broker load customer details request for customer {1} and contact email {0}", sContactEmail, nCustomerID);

			if (!User.Identity.IsAuthenticated || (User.Identity.Name != sContactEmail)) {
				oLog.Debug("Failed to load customer details request for customer {1} contact email {0}: not authenticated or authenticated as other user.", sContactEmail, nCustomerID);
				return Json(new { success = false, error = "Not authorised.", crm_data = (object)null, personal_data = (object)null }, JsonRequestBehavior.AllowGet);
			} // if

			BrokerCustomerDetailsActionResult oDetails;

			try {
				oDetails = m_oAppCreator.ServiceClient.BrokerLoadCustomerDetails(nCustomerID, sContactEmail);
			}
			catch (Exception e) {
				oLog.Debug(e, "Failed to load customers request for customer {1} and contact email {0}", sContactEmail, nCustomerID);
				return Json(new { success = false, error = "Failed to load customer details.", crm_data = (object)null, personal_data = (object)null }, JsonRequestBehavior.AllowGet);
			} // try

			oLog.Debug("Broker load customer details request for customer {1} and contact email {0} complete.", sContactEmail, nCustomerID);

			return Json(new { success = true, error = string.Empty, crm_data = oDetails.Data.CrmData, personal_data = oDetails.Data.PersonalData, }, JsonRequestBehavior.AllowGet);
		} // LoadCustomerDetails

		#endregion action LoadCustomerDetails

		#endregion public

		#region private

		#region fields

		private readonly IAppCreator m_oAppCreator;
		private readonly IEzBobConfiguration m_oConfig;

		#endregion fields

		#endregion private
	} // class BrokerHomeController
} // namespace
