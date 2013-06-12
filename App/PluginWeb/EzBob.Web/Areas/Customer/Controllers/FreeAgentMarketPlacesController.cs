namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.IO;
	using System.Net;
	using System.Web.Script.Serialization;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
    using FreeAgent;
	using FreeAgent.Config;
	using Infrastructure;
	using Scorto.Web;
	using Code.MpUniq;
	using StructureMap;
	using ZohoCRM;
	using log4net;
	using ApplicationCreator;
	using NHibernate;

    public class FreeAgentMarketPlacesController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FreeAgentMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
        private readonly ISession _session;
        private readonly DatabaseDataHelper _helper;
        private readonly IZohoFacade _crm;
		private static readonly IFreeAgentConfig config = ObjectFactory.GetInstance<IFreeAgentConfig>();


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
		public ActionResult AttachFreeAgent()
		{
			var callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");

			string authorisationRequest =
				string.Format("{0}?redirect_uri={1}&response_type=code&client_id={2}&state=xyz",
							  config.OAuthAuthorizationEndpoint, callback, config.OAuthIdentifier);

			return Redirect(authorisationRequest);
		}

		[Transactional]
		public ActionResult FreeAgentCallback()
		{
			string approvalToken = Request.QueryString["code"];
			AccessTokenContainer accessTokenContainer = null;

			// Why does this dummyCallback needed?
			var dummyCallback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");

			string accessTokenRequest = string.Format("{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
													  config.OAuthTokenEndpoint, Request.QueryString["code"], dummyCallback, config.OAuthSecret, config.OAuthIdentifier);
			var request = (HttpWebRequest)WebRequest.Create(accessTokenRequest);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			string errorMessage = null;
			try
			{
				using (var twitpicResponse = (HttpWebResponse) request.GetResponse())
				{
					var response = twitpicResponse.GetResponseStream();
					if (response != null)
					{
						using (var reader = new StreamReader(response))
						{
							var js = new JavaScriptSerializer();
							var objText = reader.ReadToEnd();
							accessTokenContainer = (AccessTokenContainer) js.Deserialize(objText, typeof (AccessTokenContainer));
						}
					}
				}
			}
			catch (Exception e)
			{
				errorMessage = "Failure getting access token";
				log.WarnFormat("{0}. Exception:{1}", errorMessage, e);
			}

			if (accessTokenContainer == null)
			{
				return View(new { error = errorMessage ?? "Failure getting access token" });
			}

			var oEsi = new FreeAgentServiceInfo();
			int marketPlaceId = _mpTypes
				.GetAll()
				.First(a => a.InternalId == oEsi.InternalId)
				.Id;

			var freeAgentCompany = FreeAgentConnector.GetCompany(accessTokenContainer.access_token);

			var securityData = new FreeAgentSecurityInfo
			{
				ApprovalToken = approvalToken,
				AccessToken = accessTokenContainer.access_token,
				ExpiresIn = accessTokenContainer.expires_in,
				TokenType = accessTokenContainer.token_type,
				RefreshToken = accessTokenContainer.refresh_token,
				MarketplaceId = marketPlaceId,
				Name = freeAgentCompany.name
			};

			var freeAgentDatabaseMarketPlace = new FreeAgentDatabaseMarketPlace();

			if (_customer.WizardStep != WizardStepType.PaymentAccounts || _customer.WizardStep != WizardStepType.AllStep)
				_customer.WizardStep = WizardStepType.Marketplace;
			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(securityData.Name, freeAgentDatabaseMarketPlace, securityData, _customer);

			_crm.ConvertLead(_customer);
			_appCreator.CustomerMarketPlaceAdded(_context.Customer, marketPlace.Id);
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