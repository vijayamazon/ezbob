namespace EzBob.Web.Areas.Customer.Controllers
{
    using CommonLib.Security;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
    using FreeAgent;
    using Infrastructure;
	using Scorto.Web;
	using Code.MpUniq;
	using Web.Models.Strings;
	using ZohoCRM;
	using log4net;
	using ApplicationCreator;
	using NHibernate;

    public class FreeAgentMarketPlacesController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FreeAgentMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
		private readonly FreeAgentConnector _validator = new FreeAgentConnector();
        private readonly ISession _session;
        private readonly DatabaseDataHelper _helper;
        private readonly IZohoFacade _crm;

		public FreeAgentMarketPlacesController(
            IEzbobWorkplaceContext context,
            DatabaseDataHelper helper,
            IRepository<MP_MarketplaceType> mpTypes,
            IMPUniqChecker mpChecker,
            IAppCreator appCreator,
            ISession session, IZohoFacade crm)
        {
            _context = context;
            _mpTypes = mpTypes;
            _customer = context.Customer;
            _mpChecker = mpChecker;
            _appCreator = appCreator;
            _session = session;
            _crm = crm;
            _helper = helper;
        }

        [Transactional]
        public JsonNetResult Accounts()
        {
			var oEsi = new FreeAgentServiceInfo();

            var freeagents = _customer
                .CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
                .Select(FreeAgentAccountModel.ToModel)
                .ToList();
            return this.JsonNet(freeagents);
        }

        [Transactional]
        [Ajax]
        [HttpPost]
		public JsonNetResult Accounts(FreeAgentAccountModel model)
        {
            string errorMsg, token;
            if (!_validator.Validate(model.displayName, out errorMsg, out token))
            {
                var errorObject = new { error = errorMsg };
                return this.JsonNet(errorObject);
            }
            try
            {
                var customer = _context.Customer;
				var username = model.displayName;
				var freeAgent = new FreeAgentDatabaseMarketPlace();

				// find out what uniq detail we can get on the account (for name and for unique check)
				//_mpChecker.Check(freeAgent.InternalId, customer, username);
				var oEsi = new FreeAgentServiceInfo();
                int marketPlaceId = _mpTypes
                    .GetAll()
                    .First(a => a.InternalId == oEsi.InternalId)
                    .Id;

				var freeAgentSecurityInfo = new FreeAgentSecurityInfo { MarketplaceId = marketPlaceId, Name = username, Token = token };

				var mp = _helper.SaveOrUpdateCustomerMarketplace(username, freeAgent, freeAgentSecurityInfo, customer);
                if (_customer.WizardStep != WizardStepType.PaymentAccounts || _customer.WizardStep != WizardStepType.AllStep)
                    _customer.WizardStep = WizardStepType.Marketplace;

                _session.Flush();

                _crm.ConvertLead(customer);
                _appCreator.CustomerMarketPlaceAdded(customer, mp.Id);

				return this.JsonNet(FreeAgentAccountModel.ToModel(mp));
            }
            catch (MarketPlaceAddedByThisCustomerException e)
            {
                Log.Debug(e);
                return this.JsonNet(new { error = DbStrings.AccountAddedByYou });
            }
            catch (MarketPlaceIsAlreadyAddedException e)
            {
                Log.Debug(e);
                return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return this.JsonNet(new { error = e.Message });
            }
        }
    }

	public class FreeAgentAccountModel
    {
        public int id { get; set; }
		public string displayName { get; set; }

		public static FreeAgentAccountModel ToModel(IDatabaseCustomerMarketPlace account)
		{
			return new FreeAgentAccountModel
				{
					id = account.Id,
					displayName = account.DisplayName
				};
		}

		public static FreeAgentAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new FreeAgentAccountModel
				{
					id = account.Id,
					displayName = account.DisplayName
				};
		} // ToModel
    }
}