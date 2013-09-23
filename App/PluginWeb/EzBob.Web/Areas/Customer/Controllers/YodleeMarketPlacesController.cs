﻿namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Collections.Generic;
	using CommonLib;
	using CommonLib.Security;
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
				yodlees.Add(YodleeAccountModel.ToModel(marketplace, new YodleeBanksRepository(_session)));
			}
			return this.JsonNet(yodlees);
		}

		[Transactional]
		public JsonNetResult Banks()
		{
			var repository = new YodleeBanksRepository(_session);
			var banks = repository.GetAll();

			var dict = new Dictionary<string, YodleeParentBankModel>();
			var yodleeBanksModel = new YodleeBanksModel
				{
					DropDownBanks = new List<YodleeSubBankModel>(),
				};

			foreach (var bank in banks)
			{
				if (bank.Active && bank.Image)
				{
					var sub = new YodleeSubBankModel {csId = bank.ContentServiceId, displayName = bank.Name};
					if (!dict.ContainsKey(bank.ParentBank))
					{
						dict.Add(bank.ParentBank, new YodleeParentBankModel { parentBankName = bank.ParentBank, subBanks = new List<YodleeSubBankModel>()});
					}
					dict[bank.ParentBank].subBanks.Add(sub);
				}

				if (bank.Active && !bank.Image)
				{
					yodleeBanksModel.DropDownBanks.Add(new YodleeSubBankModel{ csId = bank.ContentServiceId, displayName = bank.Name});
				}
			}

			yodleeBanksModel.ImageBanks= dict.Values.ToList();

			return this.JsonNet(yodleeBanksModel);
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

			string decryptedPassword = Encryptor.Decrypt(yodleeAccount.Password);
			string displayname;
			long csId;

			var yodleeMain = new YodleeMain();
			long itemId = yodleeMain.GetItemId(yodleeAccount.Username, decryptedPassword, out displayname, out csId);

			if (itemId == -1)
			{
				return View(new { error = "Failure linking account" });
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
					MarketplaceId = marketPlaceId,
					CsId = csId
				};

			var yodleeDatabaseMarketPlace = new YodleeDatabaseMarketPlace();

			if (customer.WizardStep != WizardStepType.AllStep)
				customer.WizardStep = WizardStepType.Marketplace;
			var marketPlace = _helper.SaveOrUpdateCustomerMarketplace(displayname, yodleeDatabaseMarketPlace,
				                                                        securityData, customer);

			Log.InfoFormat("Added or updated yodlee marketplace: {0}", marketPlace.Id);

			_appCreator.CustomerMarketPlaceAdded(_context.Customer, marketPlace.Id);
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
			string finalUrl = yodleeMain.GetAddAccountUrl(csId, callback, yodleeAccount.Username, Encryptor.Decrypt(yodleeAccount.Password));

			Log.InfoFormat("Redirecting to yodlee: {0}", finalUrl);
			return Redirect(finalUrl);
		}
	}

	public class YodleeAccountModel
	{
		public int bankId { get; set; }
		public string displayName { get; set; }

		public static YodleeAccountModel ToModel(IDatabaseCustomerMarketPlace marketplace, YodleeBanksRepository yodleeBanksRepository)
		{
			var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(marketplace.SecurityData);

			var yodleeBank = yodleeBanksRepository.Search(securityInfo.CsId);
			return new YodleeAccountModel
			{
				bankId = yodleeBank.Id,
				displayName = yodleeBank.Name
			};
		}

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace marketplace, YodleeBanksRepository yodleeBanksRepository)
		{
			var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(marketplace.SecurityData);

			var yodleeBank = yodleeBanksRepository.Search(securityInfo.CsId);
			return new YodleeAccountModel
			{
				bankId = yodleeBank.Id,
				displayName = yodleeBank.Name
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
		public long csId { get; set; }
		public string displayName { get; set; }
	}

	public class YodleeBanksModel
	{
		public List<YodleeParentBankModel> ImageBanks;
		public List<YodleeSubBankModel> DropDownBanks;
	}


}