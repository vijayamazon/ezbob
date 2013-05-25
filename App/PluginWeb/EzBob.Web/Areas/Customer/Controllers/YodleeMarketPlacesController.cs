namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using CommonLib.Security;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Underwriter.Models;
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
			var yodlees = new List<YodleeAccountModel>();

			foreach (var marketplace in _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId))
			{
				yodlees.Add(YodleeAccountModel.ToModel(marketplace, new YodleeAccountsRepository(_session)));
			}
			return this.JsonNet(yodlees);
		}

		[Transactional]
		public JsonNetResult Banks()
		{
			var repository = new YodleeBanksRepository(_session);
			var banks = repository.GetAll();

			var dict = new Dictionary<string, YodleeParentBankModel>();
			foreach (var bank in banks)
			{
				if (bank.Active)
				{
					var sub = new YodleeSubBankModel {csId = bank.ContentServiceId, displayName = bank.Name};
					if (!dict.ContainsKey(bank.ParentBank))
					{
						dict.Add(bank.ParentBank, new YodleeParentBankModel { parentBankName = bank.ParentBank, subBanks = new List<YodleeSubBankModel>()});
					}
					dict[bank.ParentBank].subBanks.Add(sub);
				}
			}

			var resultBanks = dict.Values.ToList();

			return this.JsonNet(resultBanks);
		}

		[Transactional]
		public ViewResult YodleeCallback()
		{
			try
			{
				var ym = new YodleeMain();
				var customer = _context.Customer;
				var repository = new YodleeAccountsRepository(_session);
				var yodleeAccount = repository.Search(customer.Id);

				// TODO: this should be run before the redirection only for customers that have existing yodlee accounts for this csid
				string decryptedPassword = Encryptor.Decrypt(yodleeAccount.Password);
				long itemId = ym.GetItemId(yodleeAccount.Username, decryptedPassword);

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
				return View(YodleeAccountModel.ToModel(marketPlace, new YodleeAccountsRepository(_session)));
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
			var repository = new YodleeAccountsRepository(_session);
			var yodleeAccount = repository.Search(_customer.Id);
			if (yodleeAccount == null)
			{
				var banksRepository = new YodleeBanksRepository(_session);
				YodleeBanks bank = banksRepository.Search(csId);
				
				// Create new account
				yodleeAccount = new YodleeAccounts
					{
						CreationDate = DateTime.UtcNow,
						Customer = _customer,
						Username = _customer.Name,
						Password = Encryptor.Encrypt(GenerateRandomPassword()),
						Bank = bank
					};

				var accountsRepository = new YodleeAccountsRepository(_session);
				int accountId = (int)accountsRepository.Save(yodleeAccount);

				Log.DebugFormat("Created yodlee account: {0}", accountId);

				yodleeMain.RegisterUser(yodleeAccount.Username, Encryptor.Decrypt(yodleeAccount.Password), _customer.Name);
			}

			var callback = Url.Action("YodleeCallback", "YodleeMarketPlaces", new { Area = "Customer" }, "https");
			string finalUrl = yodleeMain.GetFinalUrl(csId, callback, yodleeAccount.Username, Encryptor.Decrypt(yodleeAccount.Password));
			
			return Redirect(finalUrl);
		}

		private string GenerateRandomPassword()
		{
			var rnd = new Random();
			var sb = new StringBuilder();
			sb.Append(GenerateLowercaseLetter(rnd));
			sb.Append(GenerateLowercaseLetter(rnd));
			sb.Append(GenerateUppercaseLetter(rnd));
			sb.Append(GenerateLowercaseLetter(rnd));
			sb.Append(GenerateUppercaseLetter(rnd));
			sb.Append(GenerateDigit(rnd));
			sb.Append(GenerateDigit(rnd));
			sb.Append(GenerateDigit(rnd));
			return sb.ToString();
		}

		private static int GenerateDigit(Random rnd)
		{
			return rnd.Next(10);
		}

		private static char GenerateLowercaseLetter(Random rnd)
		{
			return (char)(rnd.Next(26) + 65);
		}

		private static char GenerateUppercaseLetter(Random rnd)
		{
			return (char)(rnd.Next(26) + 97);
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

		public static YodleeAccountModel ToModel(IDatabaseCustomerMarketPlace marketplace, YodleeAccountsRepository yodleeAccountsRepository)
		{
			var yodleeAccount = yodleeAccountsRepository.Search(marketplace.Customer.Id);
			return new YodleeAccountModel
			{
				bankId = yodleeAccount.Id,
				displayName = yodleeAccount.Bank.Name
			};
		}

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace marketplace, YodleeAccountsRepository yodleeAccountsRepository)
		{
			var yodleeAccount = yodleeAccountsRepository.Search(marketplace.Customer.Id);
			return new YodleeAccountModel
			{
				bankId = yodleeAccount.Id,
				displayName = yodleeAccount.Bank.Name
			};
		}
	}


	public class YodleeParentBankModel
	{
		public string parentBankName { get; set; }
		public List<YodleeSubBankModel> subBanks { get; set; }
	}

	public class YodleeSubBankModel
	{
		public int csId { get; set; }
		public string displayName { get; set; }
	}
}