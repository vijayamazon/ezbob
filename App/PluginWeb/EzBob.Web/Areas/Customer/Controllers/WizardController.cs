namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using Infrastructure.Attributes;
	using Models;
	using Infrastructure;
	using Infrastructure.Filters;
	using NHibernate;
	using NHibernate.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class WizardController : Controller {
		#region public

		#region constructor

		public WizardController(
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			ISession session,
			ICustomerReasonRepository customerReasonRepository,
			ICustomerSourceOfRepaymentRepository customerSourceOfRepaymentRepository
		) {
			_context = context;
			_questions = questions;
			_customerModelBuilder = customerModelBuilder;
			_session = session;
			_reasons = customerReasonRepository;
			_sourcesOfRepayment = customerSourceOfRepaymentRepository;
		} // constructor

		#endregion constructor

		#region action Index

		[Transactional]
		[IsSuccessfullyRegisteredFilter]
		public ActionResult Index() {
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

			var wizardModel = _customerModelBuilder.BuildWizardModel(_context.Customer);

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

		#endregion public

		#region private

		private readonly IEzbobWorkplaceContext _context;
		private readonly ISecurityQuestionRepository _questions;
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly ISession _session;
		private readonly ICustomerReasonRepository _reasons;
		private readonly ICustomerSourceOfRepaymentRepository _sourcesOfRepayment;

		#endregion private
	} // class WizardController
} // namespace
