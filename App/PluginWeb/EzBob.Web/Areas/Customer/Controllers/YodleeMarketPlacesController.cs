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
	using log4net;
	using NHibernate;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class YodleeMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly Customer _customer;
		private readonly YodleeMpUniqChecker _mpChecker;
		private readonly ServiceClient m_oServiceClient;
	    private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;

		public YodleeMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			YodleeMpUniqChecker mpChecker,
			ISession session)
		{
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			m_oServiceClient = new ServiceClient();
			_session = session;
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts()
		{
			var oEsi = new YodleeServiceInfo();
			var yodlees = new List<YodleeAccountModel>();

			foreach (var marketplace in _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId))
			{
				yodlees.Add(YodleeAccountModel.ToModel(marketplace, new YodleeBanksRepository(_session)));
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
			var customer = _context.Customer;
			var repository = new YodleeAccountsRepository(_session);
			var yodleeAccount = repository.Search(customer.Id);

			string decryptedPassword = Encrypted.Decrypt(yodleeAccount.Password);
			string displayname;
			long csId;

			var yodleeMain = new YodleeMain();
			var oEsi = new YodleeServiceInfo();

			var items = _session
				.QueryOver<MP_CustomerMarketPlace>()
				.Where(m => m.Customer.Id == customer.Id)
				.JoinQueryOver(m => m.Marketplace)
				.Where(m => m.InternalId == oEsi.InternalId)
				.List()
				.Select(m => Serialized.Deserialize<YodleeSecurityInfo>(m.SecurityData).ItemId).ToList();
				
			long itemId = yodleeMain.GetItemId(yodleeAccount.Username, decryptedPassword, items, out displayname, out csId);

			if (itemId == -1)
			{
				return View(new { error = "Failure linking account" });
			}

			int marketPlaceId = _mpTypes
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

			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(displayname, yodleeDatabaseMarketPlace, securityData, customer);

			Log.InfoFormat("Added or updated yodlee marketplace: {0}", marketPlace.Id);
			
			m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, marketPlace.Id, true, _context.UserId);

			return View(YodleeAccountModel.ToModel(marketPlace, new YodleeBanksRepository(_session)));
		}

		[Transactional]
		public ActionResult AttachYodlee(int csId, string bankName)
		{
			try
			{
				var oEsi = new YodleeServiceInfo();
				_mpChecker.Check(oEsi.InternalId, _customer, csId, _session);
			}
			catch (MarketPlaceAddedByThisCustomerException e)
			{
				Log.Debug(e);
				return View((object)DbStrings.AccountAddedByYou);
			}
			
			var yodleeMain = new YodleeMain();
			var repository = new YodleeAccountsRepository(_session);
			var yodleeAccount = repository.Search(_customer.Id);
			if (yodleeAccount == null)
			{
				var banksRepository = new YodleeBanksRepository(_session);
				YodleeBanks bank = banksRepository.Search(csId);
				yodleeAccount = YodleeAccountPool.GetAccount(_customer, bank);
			}

			var callback = Url.Action("YodleeCallback", "YodleeMarketPlaces", new { Area = "Customer" }, "https");
			string finalUrl = yodleeMain.GetAddAccountUrl(csId, callback, yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));

			Log.InfoFormat("Redirecting to yodlee: {0}", finalUrl);
			return Redirect(finalUrl);
		}
		
		[Transactional]
		public ActionResult RefreshYodlee(string displayName = null)
		{
			var customer = _context.Customer;
			var repository = new YodleeAccountsRepository(_session);
			var yodleeAccount = repository.Search(customer.Id);

			var yodleeMain = new YodleeMain();

			var oEsi = new YodleeServiceInfo();

			var yodlees = _session
				.QueryOver<MP_CustomerMarketPlace>()
				.Where(m => m.Customer.Id == customer.Id)
				.JoinQueryOver(m => m.Marketplace)
				.Where(m => m.InternalId == oEsi.InternalId)
				.List();

			if (yodlees.Count == 0)
			{
				return View(new { error = "Error Loanding Bank Accounts" });
			}
			
			var lu = yodleeMain.LoginUser(yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
			if (lu == null)
			{
				return View(new { error = "Error Loging to Yodlee Account" });
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

			m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, id, true, _context.UserId);
			return View(new {success = true});
		}
	}
}