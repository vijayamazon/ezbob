namespace EzBob.Web.Controllers {
	using System;
	using System.Web;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class HomeController : Controller {

		public HomeController(AskvilleRepository askvilleRepository) {
			m_oAskvilleRepository = askvilleRepository;
		} // constructor

		public ActionResult Index(
			string sourceref = "",
			string shop = "",
			string ezbobab = "",
			string invite = "",
			string bloken = "",
			string sourceref_time = "",
			string lead_data = "",
			string alibaba_id = "",

			string furl = "",
			string fsource = "",
			string fmedium = "",
			string fterm = "",
			string fcontent = "",
			string fname = "",
			string fdate = "",

			string rurl = "",
			string rsource = "",
			string rmedium = "",
			string rterm = "",
			string rcontent = "",
			string rname = "",
			string rdate = ""
		) {
			ms_oLog.Debug("HomeController.Index(sourceref = {0}, sourceref_time = {1})", sourceref, sourceref_time);

			ms_oLog.Debug("fulr = {0},fsource = {1},fmedium = {2},fterm = {3},fcontent = {4},fname = {5},fdate = {6},", furl,fsource,fmedium,fterm,fcontent,fname,fdate);
			ms_oLog.Debug("rulr = {0},rsource = {1},rmedium = {2},rterm = {3},rcontent = {4},rname = {5},rdate = {6},", rurl, rsource, rmedium, rterm, rcontent, rname, rdate);

			Session["Shop"] = shop;
			CreatePasswordModel oCreatePassword = null;

			if (!string.IsNullOrWhiteSpace(bloken))
				oCreatePassword = SetBrokerLeadData(bloken);

			AddCookie(sourceref, "sourceref", 3);
			AddCookie(invite, "invite", 3);
			AddCookie(ezbobab, "ezbobab", 3);
			AddCookie(sourceref_time, "sourceref_time", 3);

			AddCookie(alibaba_id, "alibaba_id", 6);

			AddCookie(furl, "furl", 6);
			AddCookie(fsource, "fsource", 6);
			AddCookie(fmedium, "fmedium", 6);
			AddCookie(fterm, "fterm", 6);
			AddCookie(fcontent, "fcontent", 6);
			AddCookie(fname, "fname", 6);
			AddCookie(fdate, "fdate", 6);

			const int week = 7;
			AddCookie(rurl, "rurl", days: week, isDay: true);
			AddCookie(rsource, "rsource", days: week, isDay: true);
			AddCookie(rmedium, "rmedium", days: week, isDay: true);
			AddCookie(rmedium, "rmedium", days: week, isDay: true);
			AddCookie(rterm, "rterm", days: week, isDay: true);
			AddCookie(rcontent, "rcontent", days: week, isDay: true);
			AddCookie(rname, "rname", days: week, isDay: true);
			AddCookie(rdate, "rdate", days: week, isDay: true);

			ParseLeadData(lead_data);

			if (oCreatePassword != null) {
				return RedirectToAction("LeadCreatePassword", "Account", new {
					sToken = oCreatePassword.RawToken,
					sFirstName = oCreatePassword.FirstName,
					sLastName = oCreatePassword.LastName,
					sEmail = oCreatePassword.UserName,
				});
			} // if

			return RedirectToActionPermanent("Index", User.Identity.IsAuthenticated ? "Profile" : "Wizard", new { Area = "Customer" });
		} // Index

		private void AddCookie(string value, string key, int months = 0, int days = 0, bool isDay = false) {
			if (!string.IsNullOrEmpty(value))
			{
				var cookie = new HttpCookie(key, value) {
					Expires = isDay ?  DateTime.Now.AddDays(days) : DateTime.Now.AddMonths(months),
					HttpOnly = true,
					Secure = true
				};
				Response.Cookies.Add(cookie);
			} // if
		}

		[Transactional]
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

		private CreatePasswordModel SetBrokerLeadData(string sBrokerLeadToken) {
			CreatePasswordModel oResult = null;

			new WizardBrokerLeadModel(Session).Unset();

			ms_oLog.Debug("Broker token observed in sign up request: {0}, processing...", sBrokerLeadToken);

			try {
				var oServiceClient = new ServiceClient();

				BrokerLeadDetailsActionResult bld = oServiceClient.Instance.BrokerLeadCheckToken(sBrokerLeadToken);

				if (bld.LeadID > 0) {
					ms_oLog.Debug(
						"Broker lead found: {0} {1} ({2}, {3}), customer id: {4}",
						bld.FirstName, 
						bld.LastName,
						bld.LeadID,
						bld.LeadEmail,
						bld.CustomerID
					);

					if (bld.CustomerID > 0) {
						oResult = new CreatePasswordModel {
							RawToken = sBrokerLeadToken,
							FirstName = bld.FirstName,
							LastName = bld.LastName,
							UserName = bld.LeadEmail,
						};
					}
					else {
						// ReSharper disable ObjectCreationAsStatement
						// This constructor saves data to Session.
						new WizardBrokerLeadModel(
							Session,
							bld.LeadID,
							bld.LeadEmail,
							bld.FirstName,
							bld.LastName,
							false
						);
						// ReSharper restore ObjectCreationAsStatement
					}
				}
				else
					ms_oLog.Debug("No lead found.");
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Something went terribly not so good while processing broker lead token {0}.", sBrokerLeadToken);
			} // try

			ms_oLog.Debug("Broker token observed in sign up request: {0}, processing complete.", sBrokerLeadToken);

			return oResult;
		} // SetBrokerLeadData

		private readonly AskvilleRepository m_oAskvilleRepository;
		private readonly ASafeLog ms_oLog = new SafeILog(typeof(HomeController));

		private void ParseLeadData(string sLeadData) {
			sLeadData = sLeadData ?? string.Empty;

			ms_oLog.Debug("Raw lead data is: '{0}'.", sLeadData);

			string[] aryPairs = sLeadData.Split(';');

			foreach (string sPair in aryPairs) {
				if (string.IsNullOrWhiteSpace(sPair))
					continue;

				int nPos = sPair.IndexOf(':');

				if (nPos < 0)
					continue;

				string sDatumName = sPair.Substring(0, nPos);

				if (string.IsNullOrWhiteSpace(sDatumName))
					continue;

				string sDatumValue = sPair.Substring(nPos + 1);

				Response.Cookies.Add(new HttpCookie("lead-datum-" + sDatumName, Url.Encode(sDatumValue)) {
					Expires = DateTime.Now.AddMonths(3),
					HttpOnly = false,
					Secure = true,
				});

				ms_oLog.Debug("Lead datum set: {0} = '{1}'.", sDatumName, sDatumValue);
			} // for each pair

		} // ParseLeadData

	} // class HomeController
} // namespace
