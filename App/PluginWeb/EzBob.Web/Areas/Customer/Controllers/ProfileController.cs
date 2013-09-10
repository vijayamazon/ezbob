using System;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.Filters;
using EzBob.Web.Infrastructure.csrf;
using EzBob.Web.Models;
using NHibernate;
using NHibernate.Linq;
using Scorto.Web;
using System.Linq;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class ProfileController : Controller
    {
        private readonly CustomerModelBuilder _customerModelBuilder;
        private readonly IEzbobWorkplaceContext _context;
        private readonly IAppCreator _creator;
        private readonly IEzBobConfiguration _config;
        private readonly CashRequestBuilder _crBuilder;
        private readonly ISession _session;

        //----------------------------------------------------------------------
        public ProfileController(
            CustomerModelBuilder customerModelBuilder, 
            IEzbobWorkplaceContext context, 
            IAppCreator creator, 
            IEzBobConfiguration config,
            CashRequestBuilder crBuilder,
            ISession session)
        {
            _customerModelBuilder = customerModelBuilder;
            _context = context;
            _creator = creator;
            _config = config;
            _crBuilder = crBuilder;
            _session = session;
        }

        //----------------------------------------------------------------------
        [IsSuccessfullyRegisteredFilter]
        [Transactional]
        public ViewResult Index()
        {
            var wizardModel = new WizardModel() {Customer = _customerModelBuilder.BuildWizardModel(_context.Customer), Config = _config};
            ViewData["ShowChangePasswordPage"] = _context.User.IsPasswordRestored;

            ViewData["ActiveMarketPlaces"] = _session
                .Query<MP_MarketplaceType>()
                .Where(x => x.Active)
                .Select(x => x.Name)
                .ToArray();

            ViewData["OfflineMarketPlaces"] = _session
                .Query<MP_MarketplaceType>()
                .Where(x => x.IsOffline)
                .Select(x => x.Name)
                .ToList();

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
            customer.OfferValidUntil = DateTime.UtcNow.AddDays(1);

            customer.ApplyCount = customer.ApplyCount + 1;

            var oldOffer = customer.LastCashRequest;
            if (oldOffer != null && oldOffer.HasLoans)
            {
                _creator.RequestCashWithoutTakenLoan(customer, Url.Action("Index", "Profile", new{Area="Customer"}));
            }

            var cashRequest = _crBuilder.CreateCashRequest(customer);

            _crBuilder.ForceEvaluate(customer, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, false);
            
            return this.JsonNet(new {});
        }


        public ViewResult RenewEbayToken()
        {
            return View();
        }
    }
}
