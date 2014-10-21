﻿namespace EzBob.Web.Controllers {
	using System;
	using System.Collections.Generic;
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
		#region public

		#region constructor

		public HomeController(AskvilleRepository askvilleRepository) {
			m_oAskvilleRepository = askvilleRepository;
		} // constructor

		#endregion constructor

		#region action Index

		public ActionResult Index(
			string sourceref = "",
			string shop = "",
			string ezbobab = "",
			string invite = "",
			string bloken = "",
			string sourceref_time = "",
			string lead_data = ""
		) {
			ms_oLog.Debug("HomeController.Index(sourceref = {0}, sourceref_time = {1})", sourceref, sourceref_time);

			Session["Shop"] = shop;
			CreatePasswordModel oCreatePassword = null;

			if (!string.IsNullOrWhiteSpace(bloken))
				oCreatePassword = SetBrokerLeadData(bloken);

			if (!string.IsNullOrEmpty(sourceref)) {
				var cookie = new HttpCookie("sourceref", sourceref) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			} // if

			if (!string.IsNullOrEmpty(invite)) {
				var cookie = new HttpCookie("invite", invite) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			} // if

			if (!string.IsNullOrEmpty(ezbobab)) {
				var cookie = new HttpCookie("ezbobab", ezbobab) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			} // if

			if (!string.IsNullOrEmpty(sourceref_time)) {
				var cookie = new HttpCookie("firstvisit", sourceref_time) { Expires = DateTime.Now.AddMonths(3), HttpOnly = true, Secure = true };
				Response.Cookies.Add(cookie);
			} // if

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

		#endregion action Index

		#region action ActivateStore

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

		#endregion action ActivateStore

		#endregion public

		#region private

		#region method SetBrokerLeadData

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

		#endregion method SetBrokerLeadData

		private readonly AskvilleRepository m_oAskvilleRepository;
		private readonly ASafeLog ms_oLog = new SafeILog(typeof(HomeController));

		#region method ParseLeadData

		private void ParseLeadData(string sLeadData) {
			sLeadData = sLeadData ?? string.Empty;

			ms_oLog.Debug("Raw lead data is: '{0}'.", sLeadData);
		} // ParseLeadData

		#endregion method ParseLeadData

		#endregion private
	} // class HomeController
} // namespace
