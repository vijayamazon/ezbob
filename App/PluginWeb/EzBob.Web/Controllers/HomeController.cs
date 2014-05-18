namespace EzBob.Web.Controllers {
	using System;
	using System.Data;
	using System.Web;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class HomeController : Controller {
		#region public

		#region constructor

		public HomeController(AskvilleRepository askvilleRepository) {
			m_oAskvilleRepository = askvilleRepository;
		} // constructor

		#endregion constructor

		#region action Index

		public ActionResult Index(string sourceref = "", string shop = "", string ezbobab = "", string invite = "", string bloken = "") {
			Session["Shop"] = shop;

			if (!string.IsNullOrWhiteSpace(bloken))
				SetBrokerLeadData(bloken);

			if (!string.IsNullOrEmpty(sourceref))
			{
				var cookie = new HttpCookie("sourceref", sourceref) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

			if (!string.IsNullOrEmpty(invite))
			{
				var cookie = new HttpCookie("invite", invite) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

			if (!string.IsNullOrEmpty(ezbobab))
			{
				var cookie = new HttpCookie("ezbobab", ezbobab) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			}

			return RedirectToActionPermanent("Index", User.Identity.IsAuthenticated ? "Profile" : "Wizard", new { Area = "Customer" });
		} // Index

		#endregion action Index

		#region action ActivateStore

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public ActionResult ActivateStore(string id, bool? approve) {
			if (approve != null) {
				var askville = m_oAskvilleRepository.GetAskvilleByGuid(id);
				var confirmStatus = (bool)approve ? AskvilleStatus.Confirmed : AskvilleStatus.NotConfirmed;

				if (askville != null) {
					askville.Status = confirmStatus;
					askville.IsPassed = true;
					m_oAskvilleRepository.SaveOrUpdate(askville);
					Utils.WriteLog("Askville confirmation", "Confirmation status " + confirmStatus.ToString(), ExperianServiceType.Askville, askville.MarketPlace.Customer.Id);
				} // if

				ViewData["Approve"] = approve;
			} // if

			return View();
		} // ActivateStore

		#endregion action ActivateStore

		#endregion public

		#region private

		#region method SetBrokerLeadData

		private void SetBrokerLeadData(string sBrokerLeadToken) {
			ASafeLog oLog = new SafeILog(LogManager.GetLogger(typeof (HomeController)));

			new WizardBrokerLeadModel(Session).Unset();

			oLog.Debug("Broker token observed in signup request: {0}, processing...", sBrokerLeadToken);

			try {
				var oServiceClient = new ServiceClient();

				BrokerLeadDetailsActionResult bld = oServiceClient.Instance.BrokerLeadCheckToken(sBrokerLeadToken);

				if (bld.LeadID > 0) {
					oLog.Debug("Broker lead found: {0} {1} ({2}, {3})", bld.FirstName, bld.LastName, bld.LeadID, bld.LeadEmail);

					// This constructor saves data to Session.
					new WizardBrokerLeadModel(
						Session,
						bld.LeadID,
						bld.LeadEmail,
						bld.FirstName,
						bld.LastName,
						false
					);
				}
				else
					oLog.Debug("No lead found.");
			}
			catch (Exception e) {
				oLog.Warn(e, "Something went terribly not so good while processing broker lead token {0}.", sBrokerLeadToken);
			} // try

			oLog.Debug("Broker token observed in signup request: {0}, processing complete.", sBrokerLeadToken);
		} // SetBrokerLeadData

		#endregion method SetBrokerLeadData

		private readonly AskvilleRepository m_oAskvilleRepository;

		#endregion private
	} // class HomeController
} // namespace
