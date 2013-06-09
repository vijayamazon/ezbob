﻿namespace EzBob.Web.Areas.Customer.Controllers
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
    using Infrastructure;
	using Scorto.Web;
	using Code.MpUniq;
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
		
		// TODO: Move these to config
		public const string OAuthIdentifier = "HeVzWPTKA70HptIMgGFl5w";
		public const string OAuthSecret = "J0BGIgMvtkMIHi5fVo94fA";
		public const string OAuthAuthorizationEndpoint = "https://api.freeagent.com/v2/approve_app";
		public const string OAuthTokenEndpoint = "https://api.freeagent.com/v2/token_endpoint";
		public const string InvoicesRequest = "https://api.freeagent.com/v2/invoices?nested_invoice_items=true";


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
							  OAuthAuthorizationEndpoint, callback, OAuthIdentifier);

			return Redirect(authorisationRequest);
		}

		[Transactional]
		public ActionResult FreeAgentCallback()
		{
			string approvalToken = Request.QueryString["code"];
			AccessTokenContainer accessTokenContainer = null;

			// Why do I have to use this dummy?
			var dummyCallback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");

			string accessTokenRequest = string.Format("{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
													  OAuthTokenEndpoint, Request.QueryString["code"], dummyCallback, OAuthSecret, OAuthIdentifier);
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
				errorMessage = string.Format("Failure getting access token. Exception:{0}", e);
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

			string accountName = "bla"; // TODO: should be filled from company api request

			var securityData = new FreeAgentSecurityInfo
			{
				ApprovalToken = approvalToken,
				AccessToken = accessTokenContainer.access_token,
				ExpiresIn = accessTokenContainer.expires_in,
				TokenType = accessTokenContainer.token_type,
				RefreshToken = accessTokenContainer.refresh_token,
				MarketplaceId = marketPlaceId,
				Name = accountName
			};

			var freeAgentDatabaseMarketPlace = new FreeAgentDatabaseMarketPlace();

			if (_customer.WizardStep != WizardStepType.PaymentAccounts || _customer.WizardStep != WizardStepType.AllStep)
				_customer.WizardStep = WizardStepType.Marketplace;
			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(accountName, freeAgentDatabaseMarketPlace, securityData, _customer);

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