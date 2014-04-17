namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Web.Mvc;
	using CommonLib.Security;
	using ConfigManager;
	using EKM;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using EzServiceReference;
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.Filters;
	using Infrastructure.csrf;
	using StructureMap;
	using Web.Models;
	using NHibernate;
	using NHibernate.Linq;
	using System.Linq;
	using EZBob.DatabaseLib.Model;

	public class ProfileController : Controller {
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ServiceClient m_oServiceClient;
		private readonly CashRequestBuilder _crBuilder;
		private readonly ISession _session;
		private readonly IConfigurationVariablesRepository _configurationVariablesRepository;

		public ProfileController(
			CustomerModelBuilder customerModelBuilder,
			IEzbobWorkplaceContext context,
			CashRequestBuilder crBuilder,
			ISession session,
			IConfigurationVariablesRepository configurationVariablesRepository
		) {
			_customerModelBuilder = customerModelBuilder;
			_context = context;
			m_oServiceClient = new ServiceClient();
			_crBuilder = crBuilder;
			_session = session;
			_configurationVariablesRepository = configurationVariablesRepository;
		} // constructor

		//----------------------------------------------------------------------
		[IsSuccessfullyRegisteredFilter]
		[Transactional]
		public ViewResult Index()
		{
			var wizardModel = new WizardModel { Customer = _customerModelBuilder.BuildWizardModel(_context.Customer) };
			ViewData["ShowChangePasswordPage"] = _context.User.IsPasswordRestored;

			ViewData["MarketPlaces"] = _session
				.Query<MP_MarketplaceType>()
				.ToArray();

			ViewData["MarketPlaceGroups"] = _session
				.Query<MP_MarketplaceGroup>()
				.ToArray();
			
			bool wizardComplete = (TempData["WizardComplete"] != null && (bool)TempData["WizardComplete"]) || (Session["WizardComplete"] != null && (bool)Session["WizardComplete"]);
			ViewData["WizardComplete"] = wizardComplete;
			Session["WizardComplete"] = false;
			TempData["WizardComplete"] = false;

			return View("Index", wizardModel);
		}

		[Transactional]
		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Details()
		{
			var details = _customerModelBuilder.BuildWizardModel(_context.Customer);
			return Json(details, JsonRequestBehavior.AllowGet);
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ClaimsTrustPilotReview() {
			var customer = _context.Customer;

			if (customer == null)
				return Json(new { status = "error", error = "Customer not found." });

			if (ReferenceEquals(customer.TrustPilotStatus, null) || customer.TrustPilotStatus.IsMe(TrustPilotStauses.Nether)) {
				var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

				customer.TrustPilotStatus = oHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Claims);

				_session.Flush();
			} // if

			return Json(new { status = "ok", error = "" });
		} // ClaimsTrustPilotReview

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetRefreshInterval()
		{
			int refreshInterval = new ServiceClient().Instance.GetCustomerStatusRefreshInterval().Value;
			return Json(new { Interval = refreshInterval });
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetCustomerStatus(int customerId)
		{
			string state = new ServiceClient().Instance.GetCustomerState(customerId).Value;
			return Json(new { State = state });
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ApplyForALoan()
		{
			var customer = _context.Customer;
			if (customer == null)
			{
				return Json(new { });
			}
			var ekmType = new EkmDatabaseMarketPlace();
			var ekms = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ekmType.InternalId).ToList();
			if (ekms.Any())
			{
				var validator = new EkmConnector();
				foreach (var ekm in ekms)
				{
					var name = ekm.DisplayName;
					var password = Encryptor.Decrypt(ekm.SecurityData);
					string error;
					var isValid = validator.Validate(name, password, out error);
					if (!isValid)
					{
						return Json(new {hasBadEkm = true, error = error, ekm = ekm.DisplayName});
					}
				}

			}
			customer.CreditResult = null;

			customer.OfferStart = DateTime.UtcNow;
			int offerValidForHours = (int)_configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");
			customer.OfferValidUntil = DateTime.UtcNow.AddHours(offerValidForHours);

			customer.ApplyCount = customer.ApplyCount + 1;

			var oldOffer = customer.LastCashRequest;
			if (oldOffer != null && oldOffer.HasLoans)
				m_oServiceClient.Instance.RequestCashWithoutTakenLoan(customer.Id);

			var cashRequest = _crBuilder.CreateCashRequest(customer, CashRequestOriginator.RequestCashBtn);

			_crBuilder.ForceEvaluate(customer.Id, customer, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, false, false);

			var yodlees = customer.GetYodleeAccounts().ToList();
			bool refreshYodleeEnabled = CurrentValues.Instance.RefreshYodleeEnabled;
			if (yodlees.Any() && refreshYodleeEnabled)
				return Json(new { hasYodlee = true });

			return Json(new { });
		}


		public ViewResult RenewEbayToken()
		{
			return View();
		}
	}
}
