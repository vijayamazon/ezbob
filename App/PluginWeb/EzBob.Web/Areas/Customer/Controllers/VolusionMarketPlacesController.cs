using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Infrastructure;
using Scorto.Web;
using Integration.Volusion;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.CommonLib.Security;
using EzBob.Web.ApplicationCreator;

namespace EzBob.Web.Areas.Customer.Controllers {
	public class VolusionAccountModel {
		public int id { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public string displayName { get; set; }
		public string url { get; set; }

		public static VolusionAccountModel ToModel(MP_CustomerMarketPlace account) {
			return new VolusionAccountModel {
				id = account.Id,
				login = account.DisplayName,
				password = Encryptor.Decrypt(account.SecurityData),
				displayName = account.DisplayName,
				url = ""
			};
		} // constructor
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

		public VolusionMarketPlacesController(
			IEzbobWorkplaceContext context,
			ICustomerRepository customers,
			IRepository<MP_MarketplaceType> mpTypes,
			IRepository<MP_CustomerMarketPlace> marketplaces,
			VolusionMPUniqChecker mpChecker,
			IAppCreator appCreator
		) {
			_context = context;
			_customers = customers;
			_mpTypes = mpTypes;
			_marketplaces = marketplaces;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
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

				var customer = _context.Customer;
				var username = model.login;
				var volusion = new VolusionDatabaseMarketPlace();

				_mpChecker.Check(volusion.InternalId, customer, username, model.url);

				var oVsi = new VolusionServiceInfo();

				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oVsi.InternalId)
					.Id;

				var mp = new MP_CustomerMarketPlace {
					Marketplace = _mpTypes.Get(marketPlaceId),
					DisplayName = model.displayName,
					SecurityData = Encryptor.EncryptBytes(model.password),
					Customer = _customer,
					Created = DateTime.UtcNow,
					UpdatingStart = DateTime.UtcNow,
					Updated = DateTime.UtcNow,
					UpdatingEnd = DateTime.UtcNow
				};

				_customer.CustomerMarketPlaces.Add(mp);
				_appCreator.EbayAdded(customer, mp.Id);
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