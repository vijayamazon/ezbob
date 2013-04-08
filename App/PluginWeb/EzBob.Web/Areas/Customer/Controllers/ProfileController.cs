using System;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
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
        private readonly ILoanTypeRepository _loanTypes;
        private readonly ISession _session;

        //----------------------------------------------------------------------
        public ProfileController(
            CustomerModelBuilder customerModelBuilder, 
            IEzbobWorkplaceContext context, 
            IAppCreator creator, 
            IEzBobConfiguration config,
            ILoanTypeRepository loanTypes,
            ISession session)
        {
            _customerModelBuilder = customerModelBuilder;
            _context = context;
            _creator = creator;
            _config = config;
            _loanTypes = loanTypes;
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

            var cashRequest = new CashRequest
            {
                CreationDate = DateTime.UtcNow,
                IdCustomer = _context.Customer.Id,
                InterestRate = 0.06m,
                RepaymentPeriod = _loanTypes.GetDefault().RepaymentPeriod,
                LoanType = _loanTypes.GetDefault(),
                OfferStart = DateTime.UtcNow,
                OfferValidUntil = DateTime.UtcNow.AddDays(1)
            };

            customer.CashRequests.Add(cashRequest);

            if (customer.CustomerMarketPlaces.Any(x => x.UpdatingEnd != null && (DateTime.UtcNow - x.UpdatingEnd.Value).Days > _config.UpdateOnReapplyLastDays))
            {
                //UpdateAllMarketplaces не успевает проставить UpdatingEnd = null для того что бы MainStrategy подождала окончание его работы
                foreach (var val in customer.CustomerMarketPlaces)
                {
                    val.UpdatingEnd = null;
                }
                _creator.UpdateAllMarketplaces(customer);
            }
            _creator.Evaluate(_context.User);
            
            return this.JsonNet(new {});
        }


        public ViewResult RenewEbayToken()
        {
            return View();
        }
    }
}
