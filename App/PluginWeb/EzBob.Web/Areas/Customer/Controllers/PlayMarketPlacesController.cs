using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;
using EzBob.Configuration;
using EzBob.Web.Infrastructure;
using Integration.ChannelGrabberAPI;
using Scorto.Web;
using Integration.Play;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using ZohoCRM;
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
	    private readonly IRepository<MP_MarketplaceType> _mpTypes;
	    private readonly EZBob.DatabaseLib.Model.Database.Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly DatabaseDataHelper _helper;
        private readonly IZohoFacade _crm;

		public PlayMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			PlayMPUniqChecker mpChecker,
			IAppCreator appCreator, IZohoFacade crm) {
			_context = context;
			_helper = helper;
		    _mpTypes = mpTypes;
		    _customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
		    _crm = crm;
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
			var ad = new PlayAccountData {
				name = model.name,
				username = model.login,
				password = model.password
			};

			var ctr = new Connector(ad, _log, _context.Customer);

			try {
				ctr.Validate();
			}
			catch (ConnectionFailChannelGrabberApiException cge) {
				if (DBConfigurationValues.Instance.ChannelGrabberRejectPolicy == ChannelGrabberRejectPolicy.ConnectionFail) {
					_log.Error(cge);
					return this.JsonNet(new {error = cge.Message});
				} // if

				_log.Error("Failed to validate Play account, continuing with registration.");
				_log.Error(cge);
			}
			catch (ChannelGrabberApiException cge) {
				_log.Error("Failed to validate Play account.");
				_log.Error(cge);
				return this.JsonNet(new { error = cge.Message });
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
                _crm.ConvertLead(customer);
				_appCreator.CustomerMarketPlaceAdded(customer, mp.Id);

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