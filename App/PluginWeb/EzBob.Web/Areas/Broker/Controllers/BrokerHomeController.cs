﻿namespace EzBob.Web.Areas.Broker.Controllers {
	using System;
	using System.Web.Mvc;
	using System.Web.Security;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;

	public class BrokerHomeController : Controller {
		#region public

		#region constructor

		public BrokerHomeController(
			DatabaseDataHelper oHelper,
			IAppCreator oAppCreator,
			IEzBobConfiguration config
		) {
			m_oHelper = oHelper;
			m_oAppCreator = oAppCreator;
			m_oConfig = config;
		} // constructor

		#endregion constructor

		#region action Index (default)

		// GET: /Broker/BrokerHome/
		public ActionResult Index() {
			const string sAuth = "auth";
			const string sForbidden = "-";

			ViewData["Config"] = m_oConfig;
			ViewData[sAuth] = string.Empty;

			if (User.Identity.IsAuthenticated) {
				Broker brkr = BrokerRepo.Find(User.Identity.Name);
				ViewData[sAuth] = ReferenceEquals(brkr, null) ? sForbidden : User.Identity.Name;
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
				m_oAppCreator.BrokerSignup(
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

		#endregion public

		#region private

		#region property BrokerRepo

		private BrokerRepository BrokerRepo {
			get { return m_oHelper.BrokerRepository; } // get
		} // BrokerRepo

		#endregion property BrokerRepo

		#region fields

		private readonly DatabaseDataHelper m_oHelper;
		private readonly IAppCreator m_oAppCreator;
		private readonly IEzBobConfiguration m_oConfig;

		#endregion fields

		#endregion private
	} // class BrokerHomeController
} // namespace
