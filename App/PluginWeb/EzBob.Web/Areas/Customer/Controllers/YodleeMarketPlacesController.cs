namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Collections.Generic;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Ezbob.Utils.Serialization;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using ServiceClientProxy;
	using Web.Models.Strings;
	using YodleeLib;
	using YodleeLib.connector;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Code.MpUniq;
	using CompanyFiles;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class YodleeMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMarketPlacesController));
		private readonly IEzbobWorkplaceContext context;
		private readonly MarketPlaceRepository mpTypes;
		private readonly Customer customer;
		private readonly YodleeMpUniqChecker mpChecker;
		private readonly ServiceClient serviceClient;
	    private readonly DatabaseDataHelper dbHelper;
		private readonly YodleeBanksRepository yodleeBanksRepository;
		private readonly YodleeAccountsRepository yodleeAccountsRepository;
		private readonly CompanyFilesMetaDataRepository companyFilesMetaDataRepository;

		public YodleeMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper dbHelper,
			MarketPlaceRepository mpTypes,
			YodleeMpUniqChecker mpChecker,
			YodleeBanksRepository yodleeBanksRepository,
			YodleeAccountsRepository yodleeAccountsRepository, 
			CompanyFilesMetaDataRepository companyFilesMetaDataRepository)
		{
			this.context = context;
			this.dbHelper = dbHelper;
			this.mpTypes = mpTypes;
			this.customer = context.Customer;
			this.mpChecker = mpChecker;
			this.serviceClient = new ServiceClient();
			this.yodleeBanksRepository = yodleeBanksRepository;
			this.yodleeAccountsRepository = yodleeAccountsRepository;
			this.companyFilesMetaDataRepository = companyFilesMetaDataRepository;
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts()
		{
			var oEsi = new YodleeServiceInfo();
			var yodlees = new List<YodleeAccountModel>();

			foreach (var marketplace in this.customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId && mp.DisplayName != "ParsedBank"))
			{
				yodlees.Add(YodleeAccountModel.ToModel(marketplace, this.yodleeBanksRepository));
			}
			return Json(yodlees, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UploadAccounts() {
			var oEsi = new YodleeServiceInfo();
			var yodlees = new List<YodleeAccountModel>();

			foreach (var marketplace in this.customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId && mp.DisplayName != "ParsedBank")) {
				yodlees.Add(YodleeAccountModel.ToModel(marketplace));
			}

			var companyFilesSI = new CompanyFilesServiceInfo();
			bool hasCompanyFilesMp = this.customer.CustomerMarketPlaces.Any(mp => mp.Marketplace.InternalId == companyFilesSI.InternalId);
			if (hasCompanyFilesMp) {
				var companyFiles = this.companyFilesMetaDataRepository.GetBankStatementFiles(this.customer.Id)
					.ToList();

				yodlees.AddRange(companyFiles.Select(x => new YodleeAccountModel { displayName = x }));
			}
			
			return Json(yodlees, JsonRequestBehavior.AllowGet);
		}

		[Transactional]
		public ViewResult YodleeCallback()
		{
			Log.InfoFormat("Got to yodlee's callback with params:{0}", HttpContext.Request.Params);
			foreach (string key in HttpContext.Request.Params.Keys)
			{
				if (key == "oauth_error_code")
				{
					Log.WarnFormat("Yodlee returned an error. oauth_error_code:{0} oauth_error_problem:{1}", HttpContext.Request.Params["oauth_error_code"], HttpContext.Request.Params["oauth_error_problem"]);
					if (HttpContext.Request.Params["oauth_error_code"] == "407")
					{
						return View(new {error = "Failure linking account"});
					}
				}
			}
			
			var yodleeAccount = this.yodleeAccountsRepository.Search(this.customer.Id);

			string decryptedPassword = Encrypted.Decrypt(yodleeAccount.Password);
			string displayname;
			long csId;

			var yodleeMain = new YodleeMain();
			var oEsi = new YodleeServiceInfo();

			var items = this.customer.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(mp => Serialized.Deserialize<YodleeSecurityInfo>(mp.SecurityData).ItemId).ToList();
				
			long itemId = yodleeMain.GetItemId(yodleeAccount.Username, decryptedPassword, items, out displayname, out csId);

			if (itemId == -1)
			{
				return View(new { error = "Failure linking account" });
			}

			int marketPlaceId = this.mpTypes
				.GetAll()
				.First(a => a.InternalId == oEsi.InternalId)
				.Id;
			
			var securityData = new YodleeSecurityInfo
				{
					ItemId = itemId,
					Name = yodleeAccount.Username,
					Password = yodleeAccount.Password,
					MarketplaceId = marketPlaceId,
					CsId = csId
				};

			var yodleeDatabaseMarketPlace = new YodleeDatabaseMarketPlace();

			var marketPlace = this.dbHelper.SaveOrUpdateCustomerMarketplace(displayname, yodleeDatabaseMarketPlace, securityData, this.customer);

			Log.InfoFormat("Added or updated yodlee marketplace: {0}", marketPlace.Id);

			this.serviceClient.Instance.UpdateMarketplace(this.context.Customer.Id, marketPlace.Id, true, this.context.UserId);

			return View(YodleeAccountModel.ToModel(marketPlace, this.yodleeBanksRepository));
		}

		[Transactional]
		public ActionResult AttachYodlee(int csId, string bankName)
		{
			try
			{
				var oEsi = new YodleeServiceInfo();
				this.mpChecker.Check(oEsi.InternalId, this.customer, csId);
			}
			catch (MarketPlaceAddedByThisCustomerException e)
			{
				Log.Debug(e);
				return View((object)DbStrings.AccountAddedByYou);
			}
			
			var yodleeMain = new YodleeMain();
			var yodleeAccount = this.yodleeAccountsRepository.Search(this.customer.Id);
			if (yodleeAccount == null)
			{
				
				YodleeBanks bank = this.yodleeBanksRepository.Search(csId);
				yodleeAccount = YodleeAccountPool.GetAccount(this.customer, bank);
			}

			var callback = Url.Action("YodleeCallback", "YodleeMarketPlaces", new { Area = "Customer" }, "https");
			string finalUrl = yodleeMain.GetAddAccountUrl(csId, callback, yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));

			Log.InfoFormat("Redirecting to yodlee: {0}", finalUrl);
			return Redirect(finalUrl);
		}
		
		[Transactional]
		public ActionResult RefreshYodlee(string displayName = null)
		{
			var yodleeAccount = this.yodleeAccountsRepository.Search(this.customer.Id);
			var yodleeMain = new YodleeMain();

			var oEsi = new YodleeServiceInfo();

			var yodlees = this.customer.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.ToList();

			if (yodlees.Count == 0)
			{
				return View(new { error = "Error loading bank accounts" });
			}
			
			var lu = yodleeMain.LoginUser(yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
			if (lu == null)
			{
				return View(new { error = "Error logging to yodlee account" });
			}

			MP_CustomerMarketPlace umi = displayName == null ? yodlees[0] : yodlees.FirstOrDefault(y => y.DisplayName == displayName); //TODO Currently refreshes the first one
			if (umi == null)
			{
				return View(new {error = "Account not found"});
			}
			var callback = Url.Action("RecheckYodleeCallback", "YodleeMarketPlaces", new { Area = "Customer" }, "https") + "/" + umi.Id;
			string finalUrl = yodleeMain.GetEditAccountUrl(Serialized.Deserialize<YodleeSecurityInfo>(umi.SecurityData).ItemId, callback, yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
			return Redirect(finalUrl);
		}

		public ActionResult RecheckYodleeCallback(int id, string oauth_token = "", string oauth_error_problem = "", string oauth_error_code = "")
		{
			if (!string.IsNullOrEmpty(oauth_error_problem) || !string.IsNullOrEmpty(oauth_error_code))
			{
				Log.ErrorFormat("Error updating yodlee mp id {0} oauth {3} {1} {2}", id, oauth_error_code, oauth_error_problem, oauth_token);
				return View(new { error = "Error occured updating bank account" });
			}

			this.serviceClient.Instance.UpdateMarketplace(this.context.Customer.Id, id, true, this.context.UserId);
			return View(new {success = true});
		}
	}
}