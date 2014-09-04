namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Marketplaces;
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

	public class WizardController : Controller {
		#region public

		#region constructor

		public WizardController(
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			ISession session,
			ICustomerReasonRepository customerReasonRepository,
			ICustomerSourceOfRepaymentRepository customerSourceOfRepaymentRepository,
			IVipRequestRepository vipRequestRepository) {
			_context = context;
			_questions = questions;
			_customerModelBuilder = customerModelBuilder;
			_session = session;
			_reasons = customerReasonRepository;
			_sourcesOfRepayment = customerSourceOfRepaymentRepository;
			_vipRequestRepository = vipRequestRepository;
			} // constructor

		#endregion constructor

		#region action Index

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

			var wizardModel = _customerModelBuilder.BuildWizardModel(_context.Customer, Session, provider);
	
			return View(wizardModel);
		} // Index

		#endregion action Index

		#region action EarnedPointsStr

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

		#endregion action EarnedPointsStr
		
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
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Vip(VipModel model)
		{
			var customer = _context.Customer;
			var vip = new VipRequest
				{
					Customer = customer,
					Email = model.VipEmail,
					FullName = model.VipFullName,
					Ip = Request.ServerVariables["REMOTE_ADDR"],
					Phone = model.VipPhone,
					RequestDate = DateTime.UtcNow
				};
			_vipRequestRepository.SaveOrUpdate(vip);
			if (customer != null)
			{
				customer.Vip = true;
			}
			var c = new ServiceClient();
			c.Instance.VipRequest(customer != null ? customer.Id : 0, model.VipFullName, model.VipEmail, model.VipPhone);
			return Json(new {});
		}

		#endregion public

		#region private

		private readonly IEzbobWorkplaceContext _context;
		private readonly ISecurityQuestionRepository _questions;
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly ISession _session;
		private readonly ICustomerReasonRepository _reasons;
		private readonly ICustomerSourceOfRepaymentRepository _sourcesOfRepayment;
		private readonly IVipRequestRepository _vipRequestRepository;

		#endregion private
	} // class WizardController
} // namespace
