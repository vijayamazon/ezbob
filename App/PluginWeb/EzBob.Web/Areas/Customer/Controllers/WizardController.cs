namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using Code;
	using Code.UiEvents;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using Infrastructure;
	using Infrastructure.Filters;
	using NHibernate;
	using NHibernate.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using Web.Models;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class WizardController : Controller {

		public WizardController(
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			ISession session,
			ICustomerReasonRepository customerReasonRepository,
			ICustomerSourceOfRepaymentRepository customerSourceOfRepaymentRepository,
			IVipRequestRepository vipRequestRepository
		) {
			_context = context;
			_questions = questions;
			_customerModelBuilder = customerModelBuilder;
			_session = session;
			_reasons = customerReasonRepository;
			_sourcesOfRepayment = customerSourceOfRepaymentRepository;
			_vipRequestRepository = vipRequestRepository;
			m_oDB = DbConnectionGenerator.Get(ms_oLog);
		} // constructor

		[IsSuccessfullyRegisteredFilter]
		public ActionResult Index(string provider = "") {
			ViewData["Questions"] = _questions.GetAll().ToList();
			ViewData["Reasons"] = _reasons.GetAll().OrderBy(x => x.Id).ToList();
			ViewData["Sources"] = _sourcesOfRepayment.GetAll().OrderBy(x => x.Id).ToList();
			ViewData["CaptchaMode"] = CurrentValues.Instance.CaptchaMode.Value;

			bool wizardTopNaviagtionEnabled = CurrentValues.Instance.WizardTopNaviagtionEnabled;
			ViewData["WizardTopNaviagtionEnabled"] = wizardTopNaviagtionEnabled;

			bool targetsEnabled = CurrentValues.Instance.TargetsEnabled;
			ViewData["TargetsEnabled"] = targetsEnabled;

			bool targetsEnabledEntrepreneur = CurrentValues.Instance.TargetsEnabledEntrepreneur;
			ViewData["TargetsEnabledEntrepreneur"] = targetsEnabledEntrepreneur;

			ViewData["MarketPlaces"] = _session
				.Query<MP_MarketplaceType>()
				.ToArray();

			ViewData["MarketPlaceGroups"] = _session
				.Query<MP_MarketplaceGroup>()
				.ToArray();

			WizardModel wizardModel = _customerModelBuilder.BuildWizardModel(_context.Customer, Session, provider);

			SavePageLoadEvent();

			return View(wizardModel);
		} // Index

		//[Ajax]
		//[HttpGet]
		//[ValidateJsonAntiForgeryToken]
		//[Transactional]
		//public JsonResult EarnedPointsStr() {
		//	var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
		//	Customer oCustomer = oDBHelper == null ? null : oDBHelper.FindCustomerByEmail(User.Identity.Name.Trim());
		//	string sPoints = "";

		//	if (oCustomer != null)
		//		sPoints = string.Format("{0:N0}", oCustomer.LoyaltyPoints());

		//	return Json(new { EarnedPointsStr = sPoints }, JsonResultBehavior.AllowGet);
		//} // EarnedPointsStr

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Vip()
		{
			var vipModel = new VipModel {VipEnabled = CurrentValues.Instance.VipEnabled};

			if (vipModel.VipEnabled)
			{
				vipModel.Ip = Request.ServerVariables["REMOTE_ADDR"];
				if (_context.Customer != null)
				{
					vipModel.VipEmail = _context.Customer.Name;
					vipModel.RequestedVip = _context.Customer.Vip;

					if (_context.Customer.PersonalInfo != null)
					{
						vipModel.VipFullName = _context.Customer.PersonalInfo.Fullname;
						vipModel.VipPhone = string.IsNullOrEmpty(_context.Customer.PersonalInfo.DaytimePhone)
							                    ? _context.Customer.PersonalInfo.MobilePhone
							                    : _context.Customer.PersonalInfo.DaytimePhone;
					}
				}
				else
				{
					var numOfRequests = _vipRequestRepository.CountRequestsPerIp(vipModel.Ip);
					if (numOfRequests >= CurrentValues.Instance.VipMaxRequests)
					{
						vipModel.RequestedVip = true;
					}
				}
			}

			return Json(vipModel, JsonRequestBehavior.AllowGet);
		} // Vip

		[Ajax]
		[HttpPost]
		
		[ValidateJsonAntiForgeryToken]
		public JsonResult Vip(VipModel model) {
			var customer = _context.Customer;
			Transactional.Execute(() => {
				var vip = new VipRequest {
					Customer = customer,
					Email = model.VipEmail,
					FullName = model.VipFullName,
					Ip = Request.ServerVariables["REMOTE_ADDR"],
					Phone = model.VipPhone,
					RequestDate = DateTime.UtcNow
				};
				_vipRequestRepository.SaveOrUpdate(vip);


				if (customer != null) {
					customer.Vip = true;
				}
			});

			var c = new ServiceClient();
			c.Instance.VipRequest(customer != null ? customer.Id : 0, model.VipFullName, model.VipEmail, model.VipPhone);
			c.Instance.SalesForceAddUpdateLeadAccount(
				customer != null ? customer.Id : (int?)null,
				model.VipEmail,
				customer != null ? customer.Id : (int?)null,
				false, customer == null);
			c.Instance.SalesForceAddTask(customer != null ? customer.Id : (int?)null, customer != null ? customer.Id : 0, new TaskModel() {
				Email = model.VipEmail,
				Originator = "System",
				CreateDate = DateTime.UtcNow,
				DueDate = DateTime.UtcNow.AddDays(1),
				Subject = "VIP request"
			});
			return Json(new {});
		} // Vip

		private readonly IEzbobWorkplaceContext _context;
		private readonly ISecurityQuestionRepository _questions;
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly ISession _session;
		private readonly ICustomerReasonRepository _reasons;
		private readonly ICustomerSourceOfRepaymentRepository _sourcesOfRepayment;
		private readonly IVipRequestRepository _vipRequestRepository;
		private readonly AConnection m_oDB;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (WizardController));

		private string GetCookie(string cookieName) {
			var reqCookie = Request.Cookies[cookieName];

			if (reqCookie != null)
				return reqCookie.Value ?? string.Empty;

			return null;
		} // GetCookie

		private void SavePageLoadEvent() {
			try {
				string sNow = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

				string sAlibabaID = GetCookie("alibaba_id");

				string sAlibabaArg = string.IsNullOrWhiteSpace(sAlibabaID) ? string.Empty : "alibaba_id:" + sAlibabaID + ";";

				var oPageLoadEvent = new UiActionEventModel {
					actionName = UiActionNames.PageLoad.ToDBName(),
					controlName = UiActionEventModel.NoControl,

					// #    #                            #
					// #   #  ###### ###### #####       # #   #      # #####    ##   #####    ##
					// #  #   #      #      #    #     #   #  #      # #    #  #  #  #    #  #  #
					// ###    #####  #####  #    #    #     # #      # #####  #    # #####  #    #
					// #  #   #      #      #####     ####### #      # #    # ###### #    # ######
					// #   #  #      #      #         #     # #      # #    # #    # #    # #    #
					// #    # ###### ###### #         #     # ###### # #####  #    # #####  #    #

					//                                                     ###
					// ##### #    # ######    ###### # #####   ####  ##### ###
					//   #   #    # #         #      # #    # #        #   ###
					//   #   ###### #####     #####  # #    #  ####    #    #
					//   #   #    # #         #      # #####       #   #
					//   #   #    # #         #      # #   #  #    #   #   ###
					//   #   #    # ######    #      # #    #  ####    #   ###

					eventArgs = sAlibabaArg + "sourceref:" + GetCookie("sourceref"),
					eventID = "0" + sNow,
					eventTime = sNow,
					htmlID = "Customer/Wizard",
				};

				int nBrowserVersionID = this.GetBrowserVersionID(Request.UserAgent ?? "Unknown browser version", m_oDB, ms_oLog);

				if (nBrowserVersionID > 0)
					oPageLoadEvent.Save(m_oDB, nBrowserVersionID, this.GetRemoteIP(), this.GetSessionID());
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to save page load event.");
			} // try
		} // SavePageLoadEvent

	} // class WizardController
} // namespace
