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
	using Web.Models;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class WizardController : Controller {

		public WizardController(
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			ISession session,
			IVipRequestRepository vipRequestRepository
		) {
			this.context = context;
			this.questions = questions;
			this.customerModelBuilder = customerModelBuilder;
			this.session = session;
			this.vipRequestRepository = vipRequestRepository;
			this.DB = DbConnectionGenerator.Get(Log);
		} // constructor

		protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
			this.hostname = requestContext.HttpContext.Request.Url.Host;
			Log.Info("WizardController Initialize {0}", this.hostname);
			base.Initialize(requestContext);
		}

		[IsSuccessfullyRegisteredFilter]
		public ActionResult Index(string provider = "") {
			ViewData["Questions"] = this.questions.GetAll().ToList();
			ViewData["CaptchaMode"] = CurrentValues.Instance.CaptchaMode.Value;
			bool wizardTopNaviagtionEnabled = CurrentValues.Instance.WizardTopNaviagtionEnabled;
			ViewData["WizardTopNaviagtionEnabled"] = wizardTopNaviagtionEnabled;

			bool targetsEnabled = CurrentValues.Instance.TargetsEnabled;
			ViewData["TargetsEnabled"] = targetsEnabled;

			bool targetsEnabledEntrepreneur = CurrentValues.Instance.TargetsEnabledEntrepreneur;
			ViewData["TargetsEnabledEntrepreneur"] = targetsEnabledEntrepreneur;

			ViewData["MarketPlaces"] = this.session
				.Query<MP_MarketplaceType>()
				.ToArray();

			ViewData["MarketPlaceGroups"] = this.session
				.Query<MP_MarketplaceGroup>()
				.ToArray();

			Log.Info("WizardController Index {0}", this.hostname);

			WizardModel wizardModel = this.customerModelBuilder.BuildWizardModel(
				this.context.Customer,
				Session,
				provider,
				this.hostname,
				false
			);

			if (TempData.ContainsKey("IsEverline")) {
				bool isEverline = (bool)TempData["IsEverline"];
				wizardModel.Customer.IsEverline = isEverline;
			}
			if (TempData.ContainsKey("CustomerEmail")) {
				wizardModel.Customer.Email = TempData["CustomerEmail"].ToString();
			}

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
				if (this.context.Customer != null)
				{
					vipModel.VipEmail = this.context.Customer.Name;
					vipModel.RequestedVip = this.context.Customer.Vip;

					if (this.context.Customer.PersonalInfo != null)
					{
						vipModel.VipFullName = this.context.Customer.PersonalInfo.Fullname;
						vipModel.VipPhone = string.IsNullOrEmpty(this.context.Customer.PersonalInfo.DaytimePhone)
							                    ? this.context.Customer.PersonalInfo.MobilePhone
							                    : this.context.Customer.PersonalInfo.DaytimePhone;
					}
				}
				else
				{
					var numOfRequests = this.vipRequestRepository.CountRequestsPerIp(vipModel.Ip);
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
			var customer = this.context.Customer;
			Transactional.Execute(() => {
				var vip = new VipRequest {
					Customer = customer,
					Email = model.VipEmail,
					FullName = model.VipFullName,
					Ip = Request.ServerVariables["REMOTE_ADDR"],
					Phone = model.VipPhone,
					RequestDate = DateTime.UtcNow
				};
				this.vipRequestRepository.SaveOrUpdate(vip);


				if (customer != null) {
					customer.Vip = true;
				}
			});

			var c = new ServiceClient();
			c.Instance.VipRequest(customer != null ? customer.Id : 0, model.VipFullName, model.VipEmail, model.VipPhone);
			return Json(new {});
		} // Vip

		private readonly IEzbobWorkplaceContext context;
		private readonly ISecurityQuestionRepository questions;
		private readonly CustomerModelBuilder customerModelBuilder;
		private readonly ISession session;
		private readonly IVipRequestRepository vipRequestRepository;
		private readonly AConnection DB;
		private static readonly ASafeLog Log = new SafeILog(typeof (WizardController));
		private string hostname;
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

				int nBrowserVersionID = this.GetBrowserVersionID(Request.UserAgent ?? "Unknown browser version", this.DB, Log);

				if (nBrowserVersionID > 0)
					oPageLoadEvent.Save(this.DB, nBrowserVersionID, this.GetRemoteIP(), this.GetSessionID());
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to save page load event.");
			} // try
		} // SavePageLoadEvent

	} // class WizardController
} // namespace
