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
		private readonly YodleeSearchWordsRepository _yodleeSearchWordsRepository;

		public MarketPlacesController(CustomerRepository customers, AnalyisisFunctionValueRepository functions, CustomerMarketPlaceRepository customerMarketplaces, MarketPlacesFacade marketPlaces, IAppCreator appCreator, MP_TeraPeakOrderItemRepository teraPeakOrderItems, YodleeAccountsRepository yodleeAccountsRepository, YodleeSearchWordsRepository yodleeSearchWordsRepository)
		{
			_customerMarketplaces = customerMarketplaces;
			_marketPlaces = marketPlaces;
			_appCreator = appCreator;
			_functions = functions;
			_customers = customers;
			_teraPeakOrderItems = teraPeakOrderItems;
			_yodleeAccountsRepository = yodleeAccountsRepository;
			_yodleeSearchWordsRepository = yodleeSearchWordsRepository;
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

			var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(mp.SecurityData);
			long itemId = securityInfo.ItemId;
			yodleeMain.LoginUser(yodleeAccount.Username, Encryptor.Decrypt(yodleeAccount.Password));

			if (!yodleeMain.IsMFA(itemId))
			{
				bool isRefreshed;
				try
				{
					isRefreshed = yodleeMain.RefreshNotMFAItem(itemId);
				}
				catch (RefreshYodleeException ex)
				{
					return View(new {error = ex.ToString()});
				}
				if (isRefreshed)
				{
					var customer = mp.Customer;
					_customerMarketplaces.ClearUpdatingEnd(umi);
					_appCreator.CustomerMarketPlaceAdded(customer, umi);
					return View();
				}
			}
			else
			{
				var callback = Url.Action("YodleeCallback", "YodleeRecheck", new {Area = "Underwriter"}, "https") + "/" + umi;
				string finalUrl = yodleeMain.GetEditAccountUrl(securityInfo.ItemId, callback, yodleeAccount.Username, Encryptor.Decrypt(yodleeAccount.Password));
				return Redirect(finalUrl);
			}

			return View();
		} // TryRecheckYodlee

		

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

		[Transactional]
		[Ajax]
		public void AddSearchWord(string word)
		{
			_yodleeSearchWordsRepository.AddWord(word);
		}

		[Transactional]
		[Ajax]
		public void DeleteSearchWord(string word)
		{
			_yodleeSearchWordsRepository.DeleteWord(word);
		}
	}
}
