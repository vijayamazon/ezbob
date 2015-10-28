namespace EzBob.Web.Areas.Customer.Controllers {
	using EZBob.DatabaseLib;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using FreeAgent;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using ServiceClientProxy;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class FreeAgentMarketPlacesController : Controller {
		public FreeAgentMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypesRepo
		) {
			this.customer = context.Customer;
			this.dbHelper = helper;
			this.mpTypesRepo = mpTypesRepo;

			this.serviceClient = new ServiceClient();
			this.connector = new FreeAgentConnector();
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts() {
			var oEsi = new FreeAgentServiceInfo();

			var freeagents = this.customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(FreeAgentAccountModel.ToModel)
				.ToList();

			return Json(freeagents, JsonRequestBehavior.AllowGet);
		} // Accounts

		public ActionResult AttachFreeAgent() {
			log.Info("Calling Attach FreeAgent URL for customer {0}", this.customer.Name);
			string callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");
			return Redirect(this.connector.GetApprovalRequest(callback));
		} // AttachFreeAgent

		public ActionResult FreeAgentCallback() {
			log.Info("Attach FreeAgent callback for customer {0}, will try to get access token...", this.customer.Name);

			string approvalToken = Request.QueryString["code"];
			string errorMessage;
			string callback = Url.Action("FreeAgentCallback", "FreeAgentMarketPlaces", new { Area = "Customer" }, "https");
			AccessTokenContainer accessTokenContainer = this.connector.GetToken(approvalToken, callback, out errorMessage);

			if (accessTokenContainer == null)
				return View(new { error = errorMessage ?? "Failed to get an access token." });

			var model = SaveFreeAgentTrn(accessTokenContainer, approvalToken);

			this.serviceClient.Instance.UpdateMarketplace(this.customer.Id, model.id, true, this.customer.Id);

			return View(model);
		} // FreeAgentCallback

		private FreeAgentAccountModel SaveFreeAgentTrn(AccessTokenContainer accessTokenContainer, string approvalToken) {
			FreeAgentAccountModel oResult = null;

			Transactional.Execute(() => {
				var oEsi = new FreeAgentServiceInfo();

				int marketPlaceId = this.mpTypesRepo
					.GetAll()
					.First(a => a.InternalId == oEsi.InternalId)
					.Id;

				log.Info("Fetching company data for customer {0}...", this.customer.Name);
				FreeAgentCompany freeAgentCompany = null;

				try {
					freeAgentCompany = this.connector.GetCompany(accessTokenContainer.access_token);
				} catch (Exception ex) {
					log.Error(ex, "Error getting FreeAgent's company. Will use customer mail as the account display name.");
				} // try

				var securityData = new FreeAgentSecurityInfo {
					ApprovalToken = approvalToken,
					AccessToken = accessTokenContainer.access_token,
					ExpiresIn = accessTokenContainer.expires_in,
					TokenType = accessTokenContainer.token_type,
					RefreshToken = accessTokenContainer.refresh_token,
					MarketplaceId = marketPlaceId,
					Name = freeAgentCompany != null ? freeAgentCompany.name : this.customer.Name,
					ValidUntil = DateTime.UtcNow.AddSeconds(accessTokenContainer.expires_in - 60)
				};

				var freeAgentDatabaseMarketPlace = new FreeAgentDatabaseMarketPlace();

				log.Info("Saving marketplace data...");
				var marketPlace = this.dbHelper.SaveOrUpdateCustomerMarketplace(
					securityData.Name,
					freeAgentDatabaseMarketPlace,
					securityData,
					this.customer
				);

				oResult = FreeAgentAccountModel.ToModel(marketPlace);
			});

			return oResult;
		} // SaveFreeAgentTrn

		private readonly MarketPlaceRepository mpTypesRepo;
		private readonly Customer customer;
		private readonly ServiceClient serviceClient;
		private readonly DatabaseDataHelper dbHelper;
		private readonly FreeAgentConnector connector;

		private static readonly ASafeLog log = new SafeILog(typeof(FreeAgentMarketPlacesController));
	} // class FreeAgentMarketPlacesController
} // namespace
