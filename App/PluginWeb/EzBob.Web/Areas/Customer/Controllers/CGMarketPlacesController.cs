using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Configuration;
using EzBob.Web.Infrastructure;
using Integration.ChannelGrabberConfig;
using Integration.ChannelGrabberFrontend;
using Scorto.Web;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.Web.ApplicationCreator;

namespace EzBob.Web.Areas.Customer.Controllers {
	using NHibernate;

	public class CGMarketPlacesController : Controller {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CGMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly EZBob.DatabaseLib.Model.Database.Customer _customer;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly DatabaseDataHelper _helper;
		private readonly ISession _session;

		public CGMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session,
			IAppCreator appCreator) {
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
			_session = session;
		} // constructor

		[Transactional]
		public JsonNetResult Accounts(string atn) {
			var oVsi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(atn);

			return this.JsonNet(_customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oVsi.Guid())
				.Select(AccountModel.ToModel)
				.ToList()
			);
		} // Accounts

		[Transactional]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(AccountModel model) {
			var oVendorInfo = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(model.accountTypeName);

			if (oVendorInfo == null) {
				var sError = "Unsupported account type: " + model.accountTypeName;
				Log.Error(sError);
				return this.JsonNet(new { error = sError });
			} // try

			AccountData ad = null;
			IMarketplaceType mktPlace = null;

			try {
				ad = model.Fill();

				mktPlace = new DatabaseMarketPlace(model.accountTypeName);

				_mpChecker.Check(mktPlace.InternalId, _context.Customer, ad.UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException ) {
				return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
			}
			catch (MarketPlaceIsAlreadyAddedException ) {
				return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
			}
			catch (Exception e) {
				Log.Error(e);
				return this.JsonNet(new { error = e.Message });
			} // try

			try {
				var ctr = new Connector(ad, Log, _context.Customer);

				if (ctr.Init()) {
					ctr.Run(true);
					ctr.Done();
				} // if
			}
			catch (ConnectionFailException cge) {
				if (DBConfigurationValues.Instance.ChannelGrabberRejectPolicy == ChannelGrabberRejectPolicy.ConnectionFail) {
					Log.Error(cge);
					return this.JsonNet(new { error = cge.Message });
				} // if

				Log.ErrorFormat("Failed to validate {0} account, continuing with registration.", model.accountTypeName);
				Log.Error(cge);
			}
			catch (ApiException cge) {
				Log.ErrorFormat("Failed to validate {0} account.", model.accountTypeName);
				Log.Error(cge);
				return this.JsonNet(new { error = cge.Message });
			}
			catch (InvalidCredentialsException ice) {
				Log.Info(ice);
				return this.JsonNet(new { error = ice.Message });
			}
			catch (Exception e) {
				Log.Error(e);
				return this.JsonNet(new { error = e.Message });
			} // try

			try {
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oVendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				if (_context.Customer.WizardStep != WizardStepType.AllStep)
					_context.Customer.WizardStep = WizardStepType.Marketplace;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(model.name, mktPlace, model, _context.Customer);
				_session.Flush();
				_appCreator.CustomerMarketPlaceAdded(_context.Customer, mp.Id);
				return this.JsonNet(AccountModel.ToModel(mp));
			}
			catch (Exception e) {
				Log.Error(e);
				return this.JsonNet(new { error = e.Message });
			} // try
		} // Accounts
	} // class CGMarketPlacesController
} // namespace