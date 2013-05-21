namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using CommonLib.Security;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
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
		private readonly IMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly YodleeConnector _validator = new YodleeConnector();
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;

		public YodleeMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper, 
			IRepository<MP_MarketplaceType> mpTypes,
			IMPUniqChecker mpChecker,
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

			var yodlees = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(YodleeAccountModel.ToModel)
				.ToList();
			return this.JsonNet(yodlees);
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(YodleeAccountModel model)
		{
			//string errorMsg;
			//if (!_validator.Validate(model.login, model.password, out errorMsg))
			//{
			//	var errorObject = new { error = errorMsg };
			//	return this.JsonNet(errorObject);
			//}
			//try
			//{
			//	var customer = _context.Customer;
			//	var username = model.login;
			//	var Yodlee = new YodleeDatabaseMarketPlace();
			//	_mpChecker.Check(Yodlee.InternalId, customer, username);
			//	var oEsi = new YodleeServiceInfo();
			//	int marketPlaceId = _mpTypes
			//		.GetAll()
			//		.First(a => a.InternalId == oEsi.InternalId)
			//		.Id;

			//	var mp = new MP_CustomerMarketPlace
			//				 {
			//					 Marketplace = _mpTypes.Get(marketPlaceId),
			//					 DisplayName = model.login,
			//					 SecurityData = Encryptor.EncryptBytes(model.password),
			//					 Customer = _customer,
			//					 Created = DateTime.UtcNow,
			//					 UpdatingStart = DateTime.UtcNow,
			//					 Updated = DateTime.UtcNow,
			//					 UpdatingEnd = DateTime.UtcNow
			//				 };

			//	_customer.CustomerMarketPlaces.Add(mp); 

			//	if (_customer.WizardStep != WizardStepType.PaymentAccounts || _customer.WizardStep != WizardStepType.AllStep)
			//		_customer.WizardStep = WizardStepType.Marketplace;

			//	_session.Flush();
			//	_appCreator.CustomerMarketPlaceAdded(customer, mp.Id);

			//	return this.JsonNet(YodleeAccountModel.ToModel(mp));
			//}
			//catch (MarketPlaceAddedByThisCustomerException e)
			//{
			//	Log.Debug(e);
			//	return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
			//}
			//catch (MarketPlaceIsAlreadyAddedException e)
			//{
			//	Log.Debug(e);
			//	return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
			//}
			//catch (Exception e)
			//{
			//	Log.Error(e);
			//	return this.JsonNet(new { error = e.Message });
			//}
			return this.JsonNet(new { error = "not implemented" });
		}

		[Transactional]
		public ViewResult YodleeCallback()
		{
			var ym = new YodleeMain();
			var customer = _context.Customer;

			var yodleeAccount = customer.YodleeAccounts.FirstOrDefault();
			
			// TODO: this should be run before the redirection only for customers that have existing yodlee accounts for this csid
			long itemId = ym.GetItemId(yodleeAccount.Username, yodleeAccount.Password);
			
			if (itemId == -1)
			{
				throw new Exception("yuly test");
				//return View("Error", (object)"Failed linking account");
			}

			var oEsi = new YodleeServiceInfo();
			int marketPlaceId = _mpTypes
				.GetAll()
				.First(a => a.InternalId == oEsi.InternalId)
				.Id;

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
			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(yodleeAccount.Username, yodleeDatabaseMarketPlace, securityData, customer);
			
			_appCreator.CustomerMarketPlaceAdded(_context.Customer, marketPlace.Id);

			return View();
		}

		[Transactional]
		public RedirectResult AttachYodlee(int csId, string bankName)
		{
			var yodleeMain = new YodleeMain();
			var x = _customer.YodleeAccounts;
			YodleeAccounts yodleeAccount;
			if (x.FirstOrDefault() == null)
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

				yodleeMain.RegisterUser(yodleeAccount.Username, yodleeAccount.Password, _customer.Name);
			}
			else
			{
				yodleeAccount = x.First();
			}

			var callback = Url.Action("YodleeCallback", "YodleeMarketPlaces", new { Area = "Customer" }, "https");
			string finalUrl = yodleeMain.GetFinalUrl(csId, callback, yodleeAccount.Username, yodleeAccount.Password);
			
			return Redirect(finalUrl);
		}
	}

	public class YodleeAccountModel
	{
		public int bankId { get; set; }
		public string bankName { get; set; }
		public string displayName { get { return bankName; } }

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new YodleeAccountModel
				{
					bankId = 1, // TODO: get real id
					bankName = account.DisplayName
				};
		}
	}
}