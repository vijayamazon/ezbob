namespace EzBob.Web.Areas.Customer.Controllers {
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Infrastructure.Attributes;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using Infrastructure;
	using Code.MpUniq;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using Web.Models.Strings;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using Newtonsoft.Json;

	public class CGMarketPlacesController : Controller {

		public CGMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker
		) {
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_mpChecker = mpChecker;
			m_oServiceClient = new ServiceClient();
		} // constructor

		public JsonResult Accounts(string atn) {
			var oVsi = Configuration.Instance.GetVendorInfo(atn);

			return Json(_context.Customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oVsi.Guid())
				.Select(AccountModel.ToModel)
				.ToList(),
				JsonRequestBehavior.AllowGet
			);
		} // Accounts

		[Ajax]
		[HttpPost]
		public JsonResult Accounts(AccountModel model) {
			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null)
				return oState.Error;

			ValidateAccount(oState, model);

			if (oState.Error != null)
				return oState.Error;

			int mpId = SaveMarketplace(oState, model);

			if (mpId != -1) {
				try {
					m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, mpId, true, _context.UserId);
				}
				catch (Exception e) {
					ms_oLog.Warn(e, "Something not so excellent while updating CG marketplace with id {0}.", mpId);
				} // try
			} // if

			if (oState.Error != null)
				return oState.Error;

			return oState.Model;
		} // Accounts

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Update(string name, string password) {
			try {
				StringActionResult ar = m_oServiceClient.Instance.ValidateAndUpdateLinkedHmrcPassword(
					new Encrypted(_context.Customer.Id.ToString()),
					new Encrypted(name),
					new Encrypted(password)
				);

				return Json(new { success = string.IsNullOrWhiteSpace(ar.Value), error = ar.Value, }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Failed to update linked HMRC account password for display name {0}.", name);
				return Json(new { success = false, error = e.Message, }, JsonRequestBehavior.AllowGet);
			} // try
		} // Update

		private class AddAccountState {
			public VendorInfo VendorInfo;
			public AccountData AccountData;
			public IMarketplaceType Marketplace;
			public JsonResult Error;
			public JsonResult Model;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState() {
				VendorInfo = null;
				AccountData = null;
				Marketplace = null;
				Error = null;
				Model = null;
				CustomerMarketPlace = null;
			} // constructor
		} // class AddAccountState

		private AddAccountState ValidateModel(AccountModel model) {
			var oResult = new AddAccountState();

			oResult.VendorInfo = Configuration.Instance.GetVendorInfo(model.accountTypeName);

			if (oResult.VendorInfo == null) {
				var sError = "Unsupported account type: " + model.accountTypeName;
				ms_oLog.Error(sError);
				oResult.Error = CreateError(sError);
				return oResult;
			} // try

			try {
				oResult.AccountData = model.Fill();

				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);

				_mpChecker.Check(oResult.Marketplace.InternalId, _context.Customer, oResult.AccountData.UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException ) {
				oResult.Error = CreateError(DbStrings.StoreAddedByYou);
				return oResult;
			}
			catch (MarketPlaceIsAlreadyAddedException ) {
				oResult.Error = CreateError(DbStrings.StoreAlreadyExistsInDb);
				return oResult;
			}
			catch (Exception e) {
				ms_oLog.Error(e);
				oResult.Error = CreateError(e);
				return oResult;
			} // try

			return oResult;
		} // ValidateModel

		private void ValidateAccount(AddAccountState oState, AccountModel model) {
			try {
				var ctr = new Connector(
					oState.AccountData,
					ms_oLog,
					_context.Customer.Id,
					_context.Customer.Name
				);

				if (ctr.Init()) {
					ctr.Run(true);
					ctr.Done();
				} // if
			}
			catch (ConnectionFailException cge) {
				if (CurrentValues.Instance.ChannelGrabberRejectPolicy == ChannelGrabberRejectPolicy.ConnectionFail) {
					ms_oLog.Error(cge, "Connection failure.");
					oState.Error = CreateError(cge);
				} // if

				ms_oLog.Error(cge, "Failed to validate {0} account, continuing with registration.", model.accountTypeName);

				// Error is logged but not written into state.
			} catch (ConfigException cex) {
				if (cex.IsWarn) {
					ms_oLog.Warn(cex, "Failed to validate {0} account.", model.accountTypeName);
				} else {
					ms_oLog.Error(cex, "Failed to validate {0} account.", model.accountTypeName);
				}
				oState.Error = CreateError(cex);
			}
			catch (ApiException cge) {
				ms_oLog.Error(cge, "Failed to validate {0} account.", model.accountTypeName);
				oState.Error = CreateError(cge);
			}
			catch (InvalidCredentialsException ice) {
				ms_oLog.Info(ice, "Invalid credentials.");
				oState.Error = CreateError(ice);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Unexpected exception.");
				oState.Error = CreateError(e);
			} // try
		} // ValidateAccount

		private int SaveMarketplace(AddAccountState oState, AccountModel model) {
			int nResult = -1;

			try {
				new Transactional(() => {
					model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
					model.displayName = model.displayName ?? model.name;

					IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateEncryptedCustomerMarketplace(
						model.name,
						oState.Marketplace,
						model,
						_context.Customer
						);

					oState.Model = Json(AccountModel.ToModel(mp), JsonRequestBehavior.AllowGet);
					oState.CustomerMarketPlace = mp;
					nResult = mp.Id;
				}).Execute();
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Failed to save marketplace.");
				oState.Error = CreateError(e);
			} // try

			return nResult;
		} // SaveMarketplace

		private JsonResult CreateError(Exception ex) {
			return CreateError(ex.Message);
		} // CreateError

		private JsonResult CreateError(string sErrorMsg) {
			return Json(new { error = sErrorMsg }, JsonRequestBehavior.AllowGet);
		} // CreateError

		private JsonResult ViewError { set {
			ViewData["error"] = JsonConvert.SerializeObject(value);
		} } // ViewError

		private string ViewModel { set { ViewData["model"] = value ?? "null"; } } // ViewModel

		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly ServiceClient m_oServiceClient;
		private readonly DatabaseDataHelper _helper;

		private static readonly SafeILog ms_oLog = new SafeILog(typeof(CGMarketPlacesController));

	} // class CGMarketPlacesController
} // namespace
