namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Sage;
	using ServiceClientProxy;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class SageMarketPlacesController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SageMarketPlacesController));
		private readonly MarketPlaceRepository _mpTypes;
        private readonly Customer _customer;
        private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;
		private static readonly object sageCallbackLock = new object();

		public SageMarketPlacesController(
            IEzbobWorkplaceContext context,
            DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes
		) {
            _mpTypes = mpTypes;
            _customer = context.Customer;
            _helper = helper;
			m_oServiceClient = new ServiceClient();
		}

        [Transactional]
        public JsonResult Accounts()
        {
			var oEsi = new SageServiceInfo();

            var sageAccounts = _customer
                .CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
                .Select(SageAccountModel.ToModel)
                .ToList();
			return Json(sageAccounts, JsonRequestBehavior.AllowGet);
        }

		[Transactional]
		public ActionResult AttachSage()
		{
			log.Info("Attaching Sage");
			string callback = Url.Action("SageCallback", "SageMarketPlaces", new { Area = "Customer" }, "https");
			string url = SageConnector.GetApprovalRequest(callback);
			log.InfoFormat("Redirecting to sage: {0}", url);
			return Redirect(url);
		}

		[Transactional]
		public ActionResult SageCallback()
		{
			lock (sageCallbackLock)
			{
				var oEsi = new SageServiceInfo();

				int addSageIntervalMinutes = ConfigManager.CurrentValues.Instance.AddSageIntervalMinutes;
				MP_CustomerMarketPlace latelyAddedSage = _customer.CustomerMarketPlaces.FirstOrDefault(a => a.Marketplace.InternalId == oEsi.InternalId && a.Created.HasValue && (DateTime.UtcNow - a.Created.Value).TotalMinutes < addSageIntervalMinutes);

				if (latelyAddedSage != null)
				{
					log.WarnFormat("Can't add more than 1 sage account every {0} minutes. Added lately:{1}", addSageIntervalMinutes, latelyAddedSage.Id);
					return View(new { error = string.Format("Can't add more than 1 sage account every {0} minutes", addSageIntervalMinutes) });
				}

				log.Info("Arrived to Sage callback, will try to get access token...");
				string approvalToken = Request.QueryString["code"];
				string errorMessage;
				string callback = Url.Action("SageCallback", "SageMarketPlaces", new {Area = "Customer"}, "https");
				AccessTokenContainer accessTokenContainer = SageConnector.GetToken(approvalToken, callback, out errorMessage);
				if (accessTokenContainer == null)
				{
					return View(new {error = errorMessage ?? "Failure getting access token"});
				}
				log.Info("Successfully received access token");

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

				string accountName = string.Format("SageOne Account #{0}", _customer.CustomerMarketPlaces.Count(a => a.Marketplace.InternalId == oEsi.InternalId) + 1);

				log.Info("Saving sage marketplace data...");
				var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(accountName, sageDatabaseMarketPlace, securityData, _customer);
				log.Info("Saved sage marketplace data...");

				m_oServiceClient.Instance.UpdateMarketplace(_customer.Id, marketPlace.Id, true);

				return View(SageAccountModel.ToModel(marketPlace));
			}
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