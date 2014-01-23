namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using CommonLib.Security;
	using EKM;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using EzServiceReference;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.Filters;
	using Infrastructure.csrf;
	using StructureMap;
	using Web.Models;
	using NHibernate;
	using NHibernate.Linq;
	using Scorto.Web;
	using System.Linq;
	using EZBob.DatabaseLib.Model;

	public class ProfileController : Controller
	{
		private readonly CustomerModelBuilder _customerModelBuilder;
		private readonly IEzbobWorkplaceContext _context;
		private readonly IAppCreator _creator;
		private readonly IEzBobConfiguration _config;
		private readonly CashRequestBuilder _crBuilder;
		private readonly ISession _session;
		private readonly IConfigurationVariablesRepository _configurationVariablesRepository;
		//----------------------------------------------------------------------
		public ProfileController(
			CustomerModelBuilder customerModelBuilder,
			IEzbobWorkplaceContext context,
			IAppCreator creator,
			IEzBobConfiguration config,
			CashRequestBuilder crBuilder,
			ISession session,
			IConfigurationVariablesRepository configurationVariablesRepository)
		{
			_customerModelBuilder = customerModelBuilder;
			_context = context;
			_creator = creator;
			_config = config;
			_crBuilder = crBuilder;
			_session = session;
			_configurationVariablesRepository = configurationVariablesRepository;
		}

		//----------------------------------------------------------------------
		[IsSuccessfullyRegisteredFilter]
		[Transactional]
		public ViewResult Index()
		{
			var wizardModel = new WizardModel { Customer = _customerModelBuilder.BuildWizardModel(_context.Customer), Config = _config };
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
		public JsonNetResult Details()
		{
			var details = _customerModelBuilder.BuildWizardModel(_context.Customer);
			return this.JsonNet(details);
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult ClaimsTrustPilotReview() {
			var customer = _context.Customer;

			if (customer == null)
				return this.JsonNet(new { status = "error", error = "Customer not found." });

			if (ReferenceEquals(customer.TrustPilotStatus, null) || customer.TrustPilotStatus.IsMe(TrustPilotStauses.Nether)) {
				var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

				customer.TrustPilotStatus = oHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Claims);

				_session.Flush();
			} // if

			return this.JsonNet(new { status = "ok", error = "" });
		} // ClaimsTrustPilotReview

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult ApplyForALoan()
		{
			var customer = _context.Customer;
			if (customer == null)
			{
				return this.JsonNet(new { });
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
						return this.JsonNet(new {hasBadEkm = true, error = error, ekm = ekm.DisplayName});
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
			{
				_creator.RequestCashWithoutTakenLoan(customer, Url.Action("Index", "Profile", new { Area = "Customer" }));
			}

			var cashRequest = _crBuilder.CreateCashRequest(customer);

			_crBuilder.ForceEvaluate(customer.Id, customer, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, false, false);

			var yodlees = customer.GetYodleeAccounts().ToList();
			var config = ObjectFactory.GetInstance<IEzBobConfiguration>();
			bool refreshYodleeEnabled = config.RefreshYodleeEnabled;
			if (yodlees.Any() && refreshYodleeEnabled)
			{
				return this.JsonNet(new { hasYodlee = true });
			}

			return this.JsonNet(new { });
		}


		public ViewResult RenewEbayToken()
		{
			return View();
		}
	}
}
