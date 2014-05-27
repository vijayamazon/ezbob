namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using Code.MpUniq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Logger;
	using Infrastructure;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using ServiceClientProxy;
	using NHibernate;
	using Web.Models.Strings;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class HmrcController : Controller {
		#region public

		#region constructor

		public HmrcController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session
		) {
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_mpChecker = mpChecker;
			_session = session;
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region action SaveFile

		[HttpPost]
		public ActionResult SaveFile() {
			int nCustomerID;

			try {
				nCustomerID = _context.Customer.Id;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to fetch current customer.");
				return CreateError("Please log out and log in again.");
			} // try

			var oProcessor = new LocalHmrcFileProcessor(nCustomerID, Request.Files);
			oProcessor.Run();

			if (!string.IsNullOrWhiteSpace(oProcessor.FileCache.ErrorMsg))
				return CreateError(oProcessor.FileCache.ErrorMsg);

			if (oProcessor.FileCache.AddedCount < 1)
				return CreateError("No files were accepted.");

			IDatabaseCustomerMarketPlace mp = FindOrCreateMarketplace();

			if (mp == null) {
				ms_oLog.Alert("Marketplace neither found nor created.");
				return CreateError(string.Format("Failed to upload VAT return file{0}.", oProcessor.FileCache.AddedCount == 1 ? "" : "s"));
			} // if

			Connector.SetBackdoorData("HMRC", mp.Id, oProcessor.FileCache.Hopper);

			try {
				m_oServiceClient.Instance.MarketplaceInstantUpdate(mp.Id);
				mp.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(mp.Id);
			}
			catch (Exception e) {
				return CreateError("Account has been linked but error occurred while storing uploaded data: " + e.Message);
			} // try

			return CreateNoError();
		} // SaveFile

		#endregion action SaveFile

		#endregion public

		#region private

		#region method FindOrCreateMarketplace

		private IDatabaseCustomerMarketPlace FindOrCreateMarketplace() {
			var oHmrcInternalID = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

			MP_CustomerMarketPlace oMp = _context.Customer.CustomerMarketPlaces.FirstOrDefault(mp => {
				if (mp.Marketplace.InternalId != oHmrcInternalID)
					return false;

				if (mp.DisplayName != _context.Customer.Name)
					return false;

				return AccountModel.ToModel(mp).password == "topsecret";
			});

			if (oMp != null) {
				oMp.SetIMarketplaceType(new DatabaseMarketPlace("HMRC"));
				return oMp;
			} // if

			var model = new AccountModel {
				accountTypeName = "HMRC",
				displayName = _context.Customer.Name,
				name = _context.Customer.Name,
				login = _context.Customer.Name,
				password = "topsecret",
			};

			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null)
				return null;

			SaveMarketplace(oState, model);

			if (oState.Error != null)
				return null;

			try {
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, oState.CustomerMarketPlace.Id, true);
			}
			catch (Exception e) {
				ms_oLog.Warn(e,
					"Failed to start UpdateMarketplace strategy for customer [{0}: {1}] with marketplace id {2}," +
					" if this is the only customer marketplace underwriter should run this strategy manually" +
					" (otherwise Main strategy will be stuck).",
					_context.Customer.Id,
					_context.Customer.Name,
					oState.CustomerMarketPlace.Id
				);
			} // try

			return oState.CustomerMarketPlace;
		} // FindOrCreateMarketplace

		#endregion method FindOrCreateMarketplace

		#region class AddAccountState

		private class AddAccountState {
			public VendorInfo VendorInfo;
			public IMarketplaceType Marketplace;
			public JsonResult Error;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState() {
				VendorInfo = null;
				Marketplace = null;
				Error = null;
				CustomerMarketPlace = null;
			} // constructor
		} // class AddAccountState

		#endregion class AddAccountState

		#region method ValidateModel

		private AddAccountState ValidateModel(AccountModel model) {
			var oResult = new AddAccountState { VendorInfo = Configuration.Instance.GetVendorInfo(model.accountTypeName) };

			if (oResult.VendorInfo == null) {
				var sError = "Unsupported account type: " + model.accountTypeName;
				ms_oLog.Error(sError);
				oResult.Error = CreateError(sError);
				return oResult;
			} // try

			try {
				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				_mpChecker.Check(oResult.Marketplace.InternalId, _context.Customer, model.Fill().UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException) {
				oResult.Error = CreateError(DbStrings.StoreAddedByYou);
				return oResult;
			}
			catch (MarketPlaceIsAlreadyAddedException) {
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

		#endregion method ValidateModel

		#region method SaveMarketplace

		private void SaveMarketplace(AddAccountState oState, AccountModel model) {
			try {
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateEncryptedCustomerMarketplace(
					model.name,
					oState.Marketplace,
					model,
					_context.Customer
				);
				_session.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				ms_oLog.Error(e);
				oState.Error = CreateError(e);
			} // try
		} // SaveMarketplace

		#endregion method SaveMarketplace

		#region method CreateError

		private JsonResult CreateError(Exception ex) {
			return CreateError(ex.Message);
		} // CreateError

		private JsonResult CreateError(string sErrorMsg) {
			if (string.IsNullOrWhiteSpace(sErrorMsg))
				return Json(new { success = true, error = string.Empty, });

			ms_oLog.Warn("Returning error from HMRC controller to web UI: {0}", sErrorMsg);
			return Json(new { success = false, error = sErrorMsg, });
		} // CreateError

		private JsonResult CreateNoError() {
			return CreateError((string)null);
		} // CreateNoError

		#endregion method CreateError

		#region fields

		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession _session;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(HmrcController));

		#endregion fields

		#endregion private
	} // class HmrcController
} // namespace
