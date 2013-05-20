using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.CommonLib;
using EzBob.Web.Infrastructure;
using Integration.ChannelGrabberAPI;
using Scorto.Web;
using Integration.Play;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.Web.ApplicationCreator;

namespace EzBob.Web.Areas.Customer.Controllers {
	public class PlayAccountModel {
		public int id { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public string name { get; set; }

		public static PlayAccountModel ToModel(MP_CustomerMarketPlace account) {
			PlaySecurityInfo oSecInfo = SerializeDataHelper.DeserializeType<PlaySecurityInfo>(account.SecurityData);

			return new PlayAccountModel {
				id = account.Id,
				login = oSecInfo.Login,
				password = oSecInfo.Password,
				name = oSecInfo.Name,
			};
		} // ToModel

		public static PlayAccountModel ToModel(IDatabaseCustomerMarketPlace account) {
			PlaySecurityInfo oSecInfo = SerializeDataHelper.DeserializeType<PlaySecurityInfo>(account.SecurityData);

			return new PlayAccountModel {
				id = account.Id,
				login = oSecInfo.Login,
				password = oSecInfo.Password,
				name = oSecInfo.Name,
			};
		} // ToModel
	} // class PlayAccountModel

	public class PlayMarketPlacesController : Controller {
		private static readonly ILog _log = LogManager.GetLogger(typeof(PlayMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly ICustomerRepository _customers;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly IRepository<MP_CustomerMarketPlace> _marketplaces;
		private EZBob.DatabaseLib.Model.Database.Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly PlayConnector _validator = new PlayConnector();
		private readonly DatabaseDataHelper _helper;

		public PlayMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper, 
			ICustomerRepository customers,
			IRepository<MP_MarketplaceType> mpTypes,
			IRepository<MP_CustomerMarketPlace> marketplaces,
			PlayMPUniqChecker mpChecker,
			IAppCreator appCreator
		) {
			_context = context;
			_helper = helper;
			_customers = customers;
			_mpTypes = mpTypes;
			_marketplaces = marketplaces;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
		} // constructor

		[Transactional]
		public JsonNetResult Accounts() {
			var oVsi = new PlayServiceInfo();

			List<PlayAccountModel> plays = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oVsi.InternalId)
				.Select(PlayAccountModel.ToModel)
				.ToList();

			return this.JsonNet(plays);
		} // Accounts

		[Transactional]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(PlayAccountModel model) {
			try {
				_validator.Validate(_log, _context.Customer, model.name, model.login, model.password);
			}
			catch (ChannelGrabberApiException cge) {
				_log.Error("Failed to validate Play account, continuing with registration.");
				_log.Error(cge);
			}
			catch (Exception e) {
				_log.Error(e);
				return this.JsonNet(new { error = e.Message });
			} // try

			try {
				var customer = _context.Customer;
				var username = model.login;
				var play = new PlayDatabaseMarketPlace();

				_mpChecker.Check(play.InternalId, customer, username, model.name);

				var oPsi = new PlayServiceInfo();

				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oPsi.InternalId)
					.Id;

				var oSecInfo = new PlaySecurityInfo {
					Name = model.name,
					Login = model.login,
					Password = model.password,
					MarketplaceId = marketPlaceId
				};

				if (customer.WizardStep != WizardStepType.PaymentAccounts || customer.WizardStep != WizardStepType.AllStep)
					customer.WizardStep = WizardStepType.Marketplace;
				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(username, play, oSecInfo, customer);

				_appCreator.CustomerMarketPlaceAdded(customer, mp.Id); // TODO: implement and use PlayAdded

				return this.JsonNet(PlayAccountModel.ToModel(mp));
			}
			catch (MarketPlaceAddedByThisCustomerException e) {
				return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
			}
			catch (MarketPlaceIsAlreadyAddedException e) {
				return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
			}
			catch (Exception e) {
				_log.Error(e);
				return this.JsonNet(new { error = e.Message });
			} // try
		} // Accounts
	} // class PlayMarketPlacesController
} // namespace