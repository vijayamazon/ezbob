﻿namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Scorto.Web;
	using EKM;
	using Code.MpUniq;
	using Web.Models.Strings;
	using log4net;
	using NHibernate;
	using System.Data;
	using Code.ApplicationCreator;
	using CommonLib.Security;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class EkmMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly EkmConnector _validator = new EkmConnector();
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;

		public EkmMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			IMPUniqChecker mpChecker,
			IAppCreator appCreator,
			ISession session)
		{
			_context = context;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
			_session = session;
			_helper = helper;
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult Accounts()
		{
			var oEsi = new EkmServiceInfo();

			var ekms = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(EkmAccountModel.ToModel)
				.ToList();
			return this.JsonNet(ekms);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(EkmAccountModel model)
		{
			string errorMsg;
			if (!_validator.Validate(model.login, model.password, out errorMsg))
			{
				var errorObject = new { error = errorMsg };
				return this.JsonNet(errorObject);
			}
			try
			{
				var customer = _context.Customer;
				var username = model.login;
				var ekm = new EkmDatabaseMarketPlace();
				_mpChecker.Check(ekm.InternalId, customer, username);
				var oEsi = new EkmServiceInfo();
				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oEsi.InternalId)
					.Id;

				var ekmSecurityInfo = new EkmSecurityInfo { MarketplaceId = marketPlaceId, Name = username, Password = model.password };

				var mp = _helper.SaveOrUpdateCustomerMarketplace(username, ekm, ekmSecurityInfo.Password, customer);

				_session.Flush();

				_appCreator.CustomerMarketPlaceAdded(customer, mp.Id);

				return this.JsonNet(EkmAccountModel.ToModel(mp));
			}
			catch (MarketPlaceAddedByThisCustomerException e)
			{
				Log.Debug(e);
				return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
			}
			catch (MarketPlaceIsAlreadyAddedException e)
			{
				Log.Debug(e);
				return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
			}
			catch (Exception e)
			{
				Log.Error(e);
				return this.JsonNet(new { error = e.Message });
			}
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		public JsonNetResult Update(string name, string password)
		{
			string errorMsg;
			if (!_validator.Validate(name, password, out errorMsg))
			{
				var errorObject = new { error = errorMsg };
				return this.JsonNet(errorObject);
			}
			try
			{
				var customer = _context.Customer;
				var ekm = new EkmDatabaseMarketPlace();
				_helper.SaveOrUpdateCustomerMarketplace(name, ekm, password, customer);
				_session.Flush();
				return this.JsonNet(new { success = true });
			}
			catch (Exception e)
			{
				Log.Error(e);
				return this.JsonNet(new { error = e.Message });
			}
		}
	}

	public class EkmAccountModel
	{
		public int id { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public string displayName { get { return login; } }

		public static EkmAccountModel ToModel(IDatabaseCustomerMarketPlace account)
		{
			return new EkmAccountModel
					   {
						   id = account.Id,
						   login = account.DisplayName,
						   password = Encryptor.Decrypt(account.SecurityData),
					   };
		}

		public static EkmAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new EkmAccountModel
			{
				id = account.Id,
				login = account.DisplayName,
				password = Encryptor.Decrypt(account.SecurityData),
			};
		} // ToModel
	}
}