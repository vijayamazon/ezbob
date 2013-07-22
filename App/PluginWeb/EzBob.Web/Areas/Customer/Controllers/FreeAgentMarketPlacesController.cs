namespace EzBob.Web.Areas.Customer.Controllers
{
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
	using ZohoCRM;
	using log4net;
	using ApplicationCreator;

    public class FreeAgentMarketPlacesController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FreeAgentMarketPlacesController));
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly Customer _customer;
        private readonly IAppCreator _appCreator;
        private readonly DatabaseDataHelper _helper;
        private readonly IZohoFacade _crm;


		public FreeAgentMarketPlacesController(
            IEzbobWorkplaceContext context,
            DatabaseDataHelper helper,
            IRepository<MP_MarketplaceType> mpTypes,
            IAppCreator appCreator,
			IZohoFacade crm)
        {
            _mpTypes = mpTypes;
            _customer = context.Customer;
            _appCreator = appCreator;
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
		public ActionResult AttachFreeAgent()
		{
			log.Info("Attaching FreeAgent");
			string callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");
			return Redirect(FreeAgentConnector.GetApprovalRequest(callback));
		}

		[Transactional]
		public ActionResult FreeAgentCallback()
		{
			log.Info("Arrived to FreeAgent callback, will try to get access token...");
			string approvalToken = Request.QueryString["code"];
			string errorMessage;
			string callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");
			AccessTokenContainer accessTokenContainer = FreeAgentConnector.GetToken(approvalToken, callback, out errorMessage);

			if (accessTokenContainer == null)
			{
				return View(new { error = errorMessage ?? "Failure getting access token" });
			}

			var oEsi = new FreeAgentServiceInfo();
			int marketPlaceId = _mpTypes
				.GetAll()
				.First(a => a.InternalId == oEsi.InternalId)
				.Id;

			log.Info("Fetching company data...");
			var freeAgentCompany = FreeAgentConnector.GetCompany(accessTokenContainer.access_token);

			var securityData = new FreeAgentSecurityInfo
			{
				ApprovalToken = approvalToken,
				AccessToken = accessTokenContainer.access_token,
				ExpiresIn = accessTokenContainer.expires_in,
				TokenType = accessTokenContainer.token_type,
				RefreshToken = accessTokenContainer.refresh_token,
				MarketplaceId = marketPlaceId,
				Name = freeAgentCompany.name,
				ValidUntil = DateTime.UtcNow.AddSeconds(accessTokenContainer.expires_in - 60)
			};

			var freeAgentDatabaseMarketPlace = new FreeAgentDatabaseMarketPlace();

			if (_customer.WizardStep != WizardStepType.AllStep)
				_customer.WizardStep = WizardStepType.Marketplace;

			log.Info("Saving marketplace data...");
			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(securityData.Name, freeAgentDatabaseMarketPlace, securityData, _customer);

			_crm.ConvertLead(_customer);
			_appCreator.CustomerMarketPlaceAdded(_customer, marketPlace.Id);
			return View(FreeAgentAccountModel.ToModel(marketPlace));
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