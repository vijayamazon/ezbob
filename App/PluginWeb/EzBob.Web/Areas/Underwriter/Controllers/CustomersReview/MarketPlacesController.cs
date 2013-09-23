namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Web.ApplicationCreator;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Models.Marketplaces;
	using Scorto.Web;
	using CommonLib;
	using CommonLib.Security;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using YodleeLib;
	using YodleeLib.connector;
	using log4net;


	public class MarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MarketPlacesController));
		private readonly CustomerRepository _customers;
		private readonly AnalyisisFunctionValueRepository _functions;
		private readonly MarketPlacesFacade _marketPlaces;
		private readonly CustomerMarketPlaceRepository _customerMarketplaces;
		private readonly IAppCreator _appCreator;
		private readonly MP_TeraPeakOrderItemRepository _teraPeakOrderItems;
		private readonly YodleeAccountsRepository _yodleeAccountsRepository;

		public MarketPlacesController(CustomerRepository customers, AnalyisisFunctionValueRepository functions, CustomerMarketPlaceRepository customerMarketplaces, MarketPlacesFacade marketPlaces, IAppCreator appCreator, MP_TeraPeakOrderItemRepository teraPeakOrderItems, YodleeAccountsRepository yodleeAccountsRepository)
		{
			_customerMarketplaces = customerMarketplaces;
			_marketPlaces = marketPlaces;
			_appCreator = appCreator;
			_functions = functions;
			_customers = customers;
			_teraPeakOrderItems = teraPeakOrderItems;
			_yodleeAccountsRepository = yodleeAccountsRepository;
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult Index(int id)
		{
			var customer = _customers.Get(id);
			var models = GetCustomerMarketplaces(customer);
			return this.JsonNet(models);
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GetTeraPeakOrderItems(int customerMarketPlaceId)
		{
			var data = _teraPeakOrderItems.GetTeraPeakOrderItems(customerMarketPlaceId);
			return this.JsonNet(data.Select(item => new Double?[2] { (ToUnixTimestamp(item.StartDate) + ToUnixTimestamp(item.EndDate)) / 2, item.Revenue }).Cast<object>().ToArray());

		}

		public static long ToUnixTimestamp(DateTime d)
		{
			var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);
			return (long)duration.TotalSeconds * 1000;
		}


		public IEnumerable<MarketPlaceModel> GetCustomerMarketplaces(EZBob.DatabaseLib.Model.Database.Customer customer)
		{
			return _marketPlaces.GetMarketPlaceModels(customer).ToList();
		}


		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult Details(int id)
		{
			var cm = _customerMarketplaces.Get(id);
			var values = _functions.GetAllValuesFor(cm);
			return this.JsonNet(values.Select(v => new FunctionValueModel(v)));
		}

		[Ajax]
		[Transactional]
		public void ReCheckMarketplaces(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);

			if (mp.UpdatingEnd == null && mp.UpdatingStart != null)
				throw new Exception("Strategy already started");

			var customer = mp.Customer;
			_customerMarketplaces.ClearUpdatingEnd(umi);

			switch (mp.Marketplace.Name)
			{
				case "Amazon":
				case "eBay":
				case "EKM":
				case "FreeAgent":
				case "Sage":
				case "PayPoint":
				case "Pay Pal":
					_appCreator.CustomerMarketPlaceAdded(customer, umi);
					break;

				default:
					if (null != Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(mp.Marketplace.Name))
						_appCreator.CustomerMarketPlaceAdded(customer, umi);
					break;
			} // switch
		} // ReCheckMarketplaces

		[Transactional]
		public ActionResult TryRecheckYodlee(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			var yodleeMain = new YodleeMain();
			var yodleeAccount = _yodleeAccountsRepository.Search(mp.Customer.Id);
			
			if (yodleeAccount == null)
			{
				return null;
			}

			string decryptedPassword = Encryptor.Decrypt(yodleeAccount.Password);
			string displayname;
			long csId;

			long itemId = yodleeMain.GetItemId(yodleeAccount.Username, decryptedPassword, out displayname, out csId);

			if (!yodleeMain.IsMFA(itemId))
			{
				if (yodleeMain.RefreshNotMFAItem(itemId))
				{
					var customer = mp.Customer;
					_customerMarketplaces.ClearUpdatingEnd(umi);
					_appCreator.CustomerMarketPlaceAdded(customer, umi);
					return null;
				}
			}
			else
			{
				var callback = Url.Action("YodleeCallback", "MarketPlaces", new {Area = "Underwriter"}, "https") + "/?umi=" + umi;
				var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(mp.SecurityData);
				string finalUrl = yodleeMain.GetEditAccountUrl(securityInfo.ItemId, callback, yodleeAccount.Username,
				                                               Encryptor.Decrypt(yodleeAccount.Password));

				return Redirect(finalUrl);
			}

			return null;
		} // TryRecheckYodlee


		[Transactional]
		public ViewResult YodleeCallback(int umi, string oauth_token = "", string oauth_error_problem = "", string oauth_error_code = "")
		{
			if (umi == -1)
			{
				return View(new { error = "Error occured (umi not found)" });
			}

			foreach (string key in HttpContext.Request.Params.Keys)
			{
				Log.InfoFormat("{0} {1}", key, HttpContext.Request.Params[key]);
				if (key == "oauth_error_code")
				{
					if (HttpContext.Request.Params["oauth_error_code"] == "407")
					{
						return View(new { error = "Error occured" });
					}
				}
			}
			//var customer = _context.Customer;
			//var repository = new YodleeAccountsRepository(_session);
			//var yodleeAccount = repository.Search(customer.Id);

			//string decryptedPassword = Encryptor.Decrypt(yodleeAccount.Password);
			//string displayname;
			//long csId;

			//var yodleeMain = new YodleeMain();
			//long itemId = yodleeMain.GetItemId(yodleeAccount.Username, decryptedPassword, out displayname, out csId);

			//if (itemId == -1)
			//{
			//	return View(new { error = "Failure linking account" });
			//}

			//var oEsi = new YodleeServiceInfo();
			//int marketPlaceId = _mpTypes
			//	.GetAll()
			//	.First(a => a.InternalId == oEsi.InternalId)
			//	.Id;

			//var securityData = new YodleeSecurityInfo
			//{
			//	ItemId = itemId,
			//	Name = yodleeAccount.Username,
			//	Password = yodleeAccount.Password,
			//	MarketplaceId = marketPlaceId,
			//	CsId = csId
			//};

			//var yodleeDatabaseMarketPlace = new YodleeDatabaseMarketPlace();

			//_appCreator.CustomerMarketPlaceAdded(_context.Customer, marketPlace.Id);
			return View(new { error = "test" });
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult CheckForUpdatedStatus(int mpId)
		{
			return this.JsonNet(new { status = _customerMarketplaces.Get(mpId).GetUpdatingStatus() });
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public void RenewEbayToken(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			var url = string.Format("https://app.ezbob.com/Customer/Profile/RenewEbayToken/");
			_appCreator.RenewEbayToken(mp.Customer, mp.DisplayName, url);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonNetResult Disable(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			mp.Disabled = true;
			return this.JsonNet(new { });
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonNetResult Enable(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			mp.Disabled = false;
			return this.JsonNet(new { });
		}
	}
}
