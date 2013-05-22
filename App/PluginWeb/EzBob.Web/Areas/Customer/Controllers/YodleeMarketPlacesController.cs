namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Web.Models.Strings;
	using YodleeLib;
	using YodleeLib.connector;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Scorto.Web;
	using Code.MpUniq;
	using log4net;
	using ApplicationCreator;
	using NHibernate;

	public class YodleeMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly Customer _customer;
		private readonly YodleeMpUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly YodleeConnector _validator = new YodleeConnector();
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;

		public YodleeMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper, 
			IRepository<MP_MarketplaceType> mpTypes,
			YodleeMpUniqChecker mpChecker,
			IAppCreator appCreator,
			ISession session)
		{
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
			_session = session;
		}

		[Transactional]
		public JsonNetResult Accounts()
		{
			var oEsi = new YodleeServiceInfo();
			List<YodleeAccountModel> yodlees = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(YodleeAccountModel.ToModel)
				.ToList();
			return this.JsonNet(yodlees);
		}

		[Transactional]
		public ViewResult YodleeCallback()
		{
			try
			{
				var ym = new YodleeMain();
				var customer = _context.Customer;

				var yodleeAccount = customer.YodleeAccounts.FirstOrDefault();

				// TODO: this should be run before the redirection only for customers that have existing yodlee accounts for this csid
				long itemId = ym.GetItemId(yodleeAccount.Username, yodleeAccount.Password);

				if (itemId == -1)
				{
					return View(new { error = "Failure linking account" });
				}

				var oEsi = new YodleeServiceInfo();
				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oEsi.InternalId)
					.Id;

				_mpChecker.Check(oEsi.InternalId, customer, itemId, _session);

				var securityData = new YodleeSecurityInfo
					{
						ItemId = itemId,
						Name = yodleeAccount.Username,
						Password = yodleeAccount.Password,
						MarketplaceId = marketPlaceId
					};

				var yodleeDatabaseMarketPlace = new YodleeDatabaseMarketPlace();

				if (customer.WizardStep != WizardStepType.PaymentAccounts || customer.WizardStep != WizardStepType.AllStep)
					customer.WizardStep = WizardStepType.Marketplace;
				var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(yodleeAccount.Username, yodleeDatabaseMarketPlace,
				                                                          securityData, customer);

				_appCreator.CustomerMarketPlaceAdded(_context.Customer, marketPlace.Id);
				return View(YodleeAccountModel.ToModel(marketPlace));
			}
			catch (MarketPlaceAddedByThisCustomerException e)
			{
				Log.Debug(e);
				return View(new {error = DbStrings.StoreAddedByYou});
			}
		}

		[Transactional]
		public RedirectResult AttachYodlee(int csId, string bankName)
		{
			var yodleeMain = new YodleeMain();
			var yodleeAccounts = _customer.YodleeAccounts;
			YodleeAccounts yodleeAccount;
			if (yodleeAccounts.FirstOrDefault() == null)
			{
				var banksRepository = new YodleeBanksRepository(_session);
				YodleeBanks bank = banksRepository.Search(csId);
				
				var rnd = new Random();
                int randomNumber = rnd.Next(9000) + 1000;
				
				// Create new account
				yodleeAccount = new YodleeAccounts
					{
						CreationDate = DateTime.UtcNow,
						Customer = _customer,
						Username = string.Format("{0}{1}", _customer.Name.Split(new [] { '@' })[0], randomNumber),
						Password = "1A4d7u",
						Bank = bank
					};

				var accountsRepository = new YodleeAccountsRepository(_session);
				int accountId = (int)accountsRepository.Save(yodleeAccount);

				Log.DebugFormat("Created yodlee account: {0}", accountId);

				yodleeMain.RegisterUser(yodleeAccount.Username, yodleeAccount.Password, _customer.Name);
			}
			else
			{
				yodleeAccount = yodleeAccounts.First();
			}

			var callback = Url.Action("YodleeCallback", "YodleeMarketPlaces", new { Area = "Customer" }, "https");
			string finalUrl = yodleeMain.GetFinalUrl(csId, callback, yodleeAccount.Username, yodleeAccount.Password);
			
			return Redirect(finalUrl);
		}
	}

	public class YodleeAccountModel
	{
		public int bankId { get; set; }
		public string displayName { get; set; }

		public static YodleeAccountModel ToModel(YodleeAccounts account)
		{
			return new YodleeAccountModel
			{
				bankId = account.Id,
				displayName = account.Bank.Name
			};
		}

		public static YodleeAccountModel ToModel(IDatabaseCustomerMarketPlace marketplace)
		{
			return new YodleeAccountModel
			{
				bankId = marketplace.Customer.YodleeAccounts.First().Id,
				displayName = marketplace.Customer.YodleeAccounts.First().Bank.Name
			};
		}

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace marketplace)
		{
			return new YodleeAccountModel
			{
				bankId = marketplace.Customer.YodleeAccounts.First().Id,
				displayName = marketplace.Customer.YodleeAccounts.First().Bank.Name
			};
		}
	}
}