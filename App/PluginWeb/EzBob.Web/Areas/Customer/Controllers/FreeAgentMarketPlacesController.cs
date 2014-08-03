namespace EzBob.Web.Areas.Customer.Controllers
{
	using EZBob.DatabaseLib;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using FreeAgent;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using ServiceClientProxy;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class FreeAgentMarketPlacesController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FreeAgentMarketPlacesController));
		private readonly MarketPlaceRepository _mpTypes;
		private readonly Customer _customer;
		private readonly ServiceClient m_oServiceClient;
		private readonly DatabaseDataHelper _helper;


		public FreeAgentMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes)
		{
			_mpTypes = mpTypes;
			_customer = context.Customer;
			m_oServiceClient = new ServiceClient();
			_helper = helper;
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts()
		{
			var oEsi = new FreeAgentServiceInfo();

			var freeagents = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(FreeAgentAccountModel.ToModel)
				.ToList();
			return Json(freeagents, JsonRequestBehavior.AllowGet);
		}

		public ActionResult AttachFreeAgent()
		{
			log.Info("Attaching FreeAgent");
			string callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");
			return Redirect(FreeAgentConnector.GetApprovalRequest(callback));
		}

		public ActionResult FreeAgentCallback()
		{
			log.Info("Arrived to FreeAgent callback, will try to get access token...");
			string approvalToken = Request.QueryString["code"];
			string errorMessage;
			string callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");
			AccessTokenContainer accessTokenContainer = FreeAgentConnector.GetToken(approvalToken, callback, out errorMessage);

			if (accessTokenContainer == null)
				return View(new { error = errorMessage ?? "Failure getting access token" });

			var model = SaveFreeAgentTrn(accessTokenContainer, approvalToken);

			m_oServiceClient.Instance.UpdateMarketplace(_customer.Id, model.id, true);

			return View(model);
		}

		private FreeAgentAccountModel SaveFreeAgentTrn(AccessTokenContainer accessTokenContainer, string approvalToken) {
			FreeAgentAccountModel oResult = null;

			Transactional.Execute(() => {
				var oEsi = new FreeAgentServiceInfo();
				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oEsi.InternalId)
					.Id;

				log.Info("Fetching company data...");
				FreeAgentCompany freeAgentCompany = null;

				try {
					freeAgentCompany = FreeAgentConnector.GetCompany(accessTokenContainer.access_token);
				}
				catch (Exception ex) {
					log.ErrorFormat("Error getting FreeAgent's company. Will use customer mail as the account display name: {0}", ex);
				}

				var securityData = new FreeAgentSecurityInfo {
					ApprovalToken = approvalToken,
					AccessToken = accessTokenContainer.access_token,
					ExpiresIn = accessTokenContainer.expires_in,
					TokenType = accessTokenContainer.token_type,
					RefreshToken = accessTokenContainer.refresh_token,
					MarketplaceId = marketPlaceId,
					Name = freeAgentCompany != null ? freeAgentCompany.name : _customer.Name,
					ValidUntil = DateTime.UtcNow.AddSeconds(accessTokenContainer.expires_in - 60)
				};

				var freeAgentDatabaseMarketPlace = new FreeAgentDatabaseMarketPlace();

				log.Info("Saving marketplace data...");
				var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(securityData.Name, freeAgentDatabaseMarketPlace, securityData, _customer);

				oResult = FreeAgentAccountModel.ToModel(marketPlace);
			});

			return oResult;
		}
	}
}