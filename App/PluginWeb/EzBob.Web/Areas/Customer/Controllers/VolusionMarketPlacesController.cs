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
using Integration.Volusion;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.CommonLib.Security;
using EzBob.Web.ApplicationCreator;

namespace EzBob.Web.Areas.Customer.Controllers {
    using NHibernate;

    public class VolusionAccountModel {
		public int id { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public string displayName { get; set; }
		public string url { get; set; }

		public static VolusionAccountModel ToModel(MP_CustomerMarketPlace account) {
			VolusionSecurityInfo oSecInfo = SerializeDataHelper.DeserializeType<VolusionSecurityInfo>(account.SecurityData);

			return new VolusionAccountModel {
				id = account.Id,
				login = oSecInfo.Login,
				password = oSecInfo.Password,
				displayName = oSecInfo.DisplayName,
				url = ""
			};
		} // ToModel

		public static VolusionAccountModel ToModel(IDatabaseCustomerMarketPlace account) {
			VolusionSecurityInfo oSecInfo = SerializeDataHelper.DeserializeType<VolusionSecurityInfo>(account.SecurityData);

			return new VolusionAccountModel {
				id = account.Id,
				login = oSecInfo.Login,
				password = oSecInfo.Password,
				displayName = oSecInfo.DisplayName,
				url = ""
			};
		} // ToModel
	} // class VolusionAccountModel

	public class VolusionMarketPlacesController : Controller {
		private static readonly ILog _log = LogManager.GetLogger(typeof(VolusionMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly ICustomerRepository _customers;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly IRepository<MP_CustomerMarketPlace> _marketplaces;
		private EZBob.DatabaseLib.Model.Database.Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly VolusionConnector _validator = new VolusionConnector();
		private readonly DatabaseDataHelper _helper;
        private readonly ISession _session;

		public VolusionMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper, 
			ICustomerRepository customers,
			IRepository<MP_MarketplaceType> mpTypes,
			IRepository<MP_CustomerMarketPlace> marketplaces,
			VolusionMPUniqChecker mpChecker,
            ISession session,
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
		    _session = session;
		} // constructor

		[Transactional]
		public JsonNetResult Accounts() {
			var oVsi = new VolusionServiceInfo();

			List<VolusionAccountModel> volusions = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oVsi.InternalId)
				.Select(VolusionAccountModel.ToModel)
				.ToList();

			return this.JsonNet(volusions);
		} // Accounts

		[Transactional]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(VolusionAccountModel model) {
			try {
				_validator.Validate(_log, _context.Customer, model.displayName, model.url, model.login, model.password);
			}
			catch (ChannelGrabberApiException cge) {
				_log.Error("Failed to validate Volusion account, continuing with registration.");
				_log.Error(cge);
			}
			catch (Exception e) {
				_log.Error(e);
				return this.JsonNet(new { error = e.Message });
			} // try

			try {
				var customer = _context.Customer;
				var username = model.login;
				var volusion = new VolusionDatabaseMarketPlace();

				_mpChecker.Check(volusion.InternalId, customer, username, model.url);

				var oVsi = new VolusionServiceInfo();

				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oVsi.InternalId)
					.Id;

				var oSecInfo = new VolusionSecurityInfo {
					Url = model.url,
					Login = model.login,
					Password = model.password,
					DisplayName = model.displayName,
					MarketplaceId = marketPlaceId
				};

				if (customer.WizardStep != WizardStepType.PaymentAccounts || customer.WizardStep != WizardStepType.AllStep)
					customer.WizardStep = WizardStepType.Marketplace;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(username, volusion, oSecInfo, customer);
                _session.Flush();
				_appCreator.CustomerMarketPlaceAdded(customer, mp.Id); 

				return this.JsonNet(VolusionAccountModel.ToModel(mp));
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
	} // class VolusionMarketPlacesController
} // namespace