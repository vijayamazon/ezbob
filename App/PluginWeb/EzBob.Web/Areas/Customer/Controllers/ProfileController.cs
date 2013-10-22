namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using ApplicationCreator;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.Filters;
	using Infrastructure.csrf;
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
		private readonly IConfigurationVariablesRepository configurationVariablesRepository;

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
			this.configurationVariablesRepository = configurationVariablesRepository;
        }

        //----------------------------------------------------------------------
        [IsSuccessfullyRegisteredFilter]
        [Transactional]
        public ViewResult Index()
        {
            var wizardModel = new WizardModel() {Customer = _customerModelBuilder.BuildWizardModel(_context.Customer), Config = _config};
            ViewData["ShowChangePasswordPage"] = _context.User.IsPasswordRestored;

            ViewData["MarketPlaces"] = _session
                .Query<MP_MarketplaceType>()
                .ToArray();

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
        public JsonNetResult ApplyForALoan()
        {
            var customer = _context.Customer;
            if(customer == null)
            {
                return this.JsonNet(new {});
            }

			customer.CreditResult = null;

			customer.OfferStart = DateTime.UtcNow;
			int offerValidForHours = (int)configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");
			customer.OfferValidUntil = DateTime.UtcNow.AddHours(offerValidForHours);

			customer.ApplyCount = customer.ApplyCount + 1;

			var oldOffer = customer.LastCashRequest;
			if (oldOffer != null && oldOffer.HasLoans)
			{
				_creator.RequestCashWithoutTakenLoan(customer, Url.Action("Index", "Profile", new { Area = "Customer" }));
			}

			var cashRequest = _crBuilder.CreateCashRequest(customer);

			_crBuilder.ForceEvaluate(customer, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, false);

	        var yodlees = customer.GetYodleeAccounts().ToList();
			if (yodlees.Any())
			{
				return this.JsonNet(new { hasYodlee = true });
			}

            return this.JsonNet(new {});
        }


        public ViewResult RenewEbayToken()
        {
            return View();
        }
    }
}
