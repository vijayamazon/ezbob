﻿namespace EzBob.Web.Areas.Customer.Controllers
{
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
    using Infrastructure;
    using Sage;
    using Scorto.Web;
	using ZohoCRM;
	using log4net;
	using ApplicationCreator;

	public class SageMarketPlacesController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SageMarketPlacesController));
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly Customer _customer;
        private readonly IAppCreator _appCreator;
        private readonly DatabaseDataHelper _helper;
        private readonly IZohoFacade _crm;

		public SageMarketPlacesController(
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
			var oEsi = new SageServiceInfo();

            var sageAccounts = _customer
                .CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
                .Select(SageAccountModel.ToModel)
                .ToList();
			return this.JsonNet(sageAccounts);
        }

		[Transactional]
		public ActionResult AttachSage()
		{
			log.Info("Attaching Sage");
			string callback = Url.Action("SageCallback", "SageMarketPlaces", new { Area = "Customer" }, "https");
			return Redirect(SageConnector.GetApprovalRequest(callback));
		}

		[Transactional]
		public ActionResult SageCallback()
		{
			log.Info("Arrived to Sage callback, will try to get access token...");
			string approvalToken = Request.QueryString["code"];
			string errorMessage;
			string callback = Url.Action("SageCallback", "SageMarketPlaces", new { Area = "Customer" }, "https");
			AccessTokenContainer accessTokenContainer = SageConnector.GetToken(approvalToken, callback, out errorMessage);

			if (accessTokenContainer == null)
			{
				return View(new { error = errorMessage ?? "Failure getting access token" });
			}

			var oEsi = new SageServiceInfo();
			int marketPlaceId = _mpTypes
				.GetAll()
				.First(a => a.InternalId == oEsi.InternalId)
				.Id;

			var securityData = new SageSecurityInfo
			{
				ApprovalToken = approvalToken,
				AccessToken = accessTokenContainer.access_token,
				TokenType = accessTokenContainer.token_type,
				MarketplaceId = marketPlaceId
			};

			var sageDatabaseMarketPlace = new SageDatabaseMarketPlace();

			if (_customer.WizardStep != WizardStepType.PaymentAccounts && _customer.WizardStep != WizardStepType.AllStep)
				_customer.WizardStep = WizardStepType.Marketplace;

			log.Info("Saving marketplace data...");
			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(_customer.Name/*qqq - get actual store name*/, sageDatabaseMarketPlace, securityData, _customer);

			_crm.ConvertLead(_customer);
			_appCreator.CustomerMarketPlaceAdded(_customer, marketPlace.Id);
			return View(SageAccountModel.ToModel(marketPlace));
		}
    }

	public class SageAccountModel
    {
        public int id { get; set; }
		public string displayName { get; set; }

		public static SageAccountModel ToModel(IDatabaseCustomerMarketPlace account)
		{
			return new SageAccountModel
				{
					id = account.Id,
					displayName = account.DisplayName
				};
		}

		public static SageAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new SageAccountModel
				{
					id = account.Id,
					displayName = account.DisplayName
				};
		} // ToModel
    }
}