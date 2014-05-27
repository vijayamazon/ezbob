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
			lock (ms_oLockVendorInfo) {
				if (ms_oVendorInfo == null)
					ms_oVendorInfo = Configuration.Instance.GetVendorInfo("HMRC");

				m_oVendorInfo = ms_oVendorInfo;
			} // lock

			m_oContext = context;
			m_oDatabaseHelper = helper;
			m_oMpTypes = mpTypes;
			m_oUniquenessChecker = mpChecker;
			m_oSession = session;
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region action SaveFile

		[HttpPost]
		public ActionResult SaveFile() {
			int nCustomerID;

			try {
				nCustomerID = m_oContext.Customer.Id;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to fetch current customer.");
				return CreateError("Please log out and log in again.");
			} // try

			var oProcessor = new HmrcFileProcessor(nCustomerID, Request.Files);
			oProcessor.Run();

			if (!string.IsNullOrWhiteSpace(oProcessor.ErrorMsg))
				return CreateError(oProcessor.ErrorMsg);

			if (oProcessor.AddedCount < 1)
				return CreateError("No files were accepted.");

			IDatabaseCustomerMarketPlace mp = FindOrCreateMarketplace();

			if (mp == null) {
				ms_oLog.Alert("Marketplace neither found nor created.");
				return CreateError(string.Format("Failed to upload VAT return file{0}.", oProcessor.AddedCount == 1 ? "" : "s"));
			} // if

			Connector.SetBackdoorData(m_oVendorInfo.Name, mp.Id, oProcessor.Hopper);

			try {
				m_oServiceClient.Instance.MarketplaceInstantUpdate(mp.Id);
				mp.Marketplace.GetRetrieveDataHelper(m_oDatabaseHelper).UpdateCustomerMarketplaceFirst(mp.Id);
			}
			catch (Exception e) {
				return CreateError("Account has been linked but error occurred while storing uploaded data: " + e.Message);
			} // try

			return CreateNoError();
		} // SaveFile

		#endregion action SaveFile

		#endregion public

		#region private

		#region method FindMarketplace

		private IDatabaseCustomerMarketPlace FindMarketplace() {
			MP_CustomerMarketPlace oMp = m_oContext.Customer.CustomerMarketPlaces.FirstOrDefault(mp => {
				if (mp.Marketplace.InternalId != m_oVendorInfo.Guid())
					return false;

				if (mp.DisplayName != m_oContext.Customer.Name)
					return false;

				return AccountModel.ToModel(mp).password == VendorInfo.TopSecret;
			});

			if (oMp != null) {
				oMp.SetIMarketplaceType(new DatabaseMarketPlace(m_oVendorInfo.Name));
				return oMp;
			} // if

			return null;
		} // FindMarketplace

		#endregion method FindMarketplace

		#region method FindOrCreateMarketplace

		private IDatabaseCustomerMarketPlace FindOrCreateMarketplace() {
			IDatabaseCustomerMarketPlace oMp = FindMarketplace();

			if (oMp != null)
				return oMp;

			lock (ms_oLockCreateMarketplace) {
				oMp = FindMarketplace();

				if (oMp != null)
					return oMp;

				var model = new AccountModel {
					accountTypeName = m_oVendorInfo.Name,
					displayName = m_oContext.Customer.Name,
					name = m_oContext.Customer.Name,
					login = m_oContext.Customer.Name,
					password = VendorInfo.TopSecret,
				};

				AddAccountState oState = ValidateMpUniqueness(model);

				if (oState.Error != null)
					return null;

				SaveMarketplace(oState, model);

				return oState.CustomerMarketPlace;
			} // lock
		} // FindOrCreateMarketplace

		#endregion method FindOrCreateMarketplace

		#region class AddAccountState

		private class AddAccountState {
			public IMarketplaceType Marketplace;
			public JsonResult Error;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState() {
				Marketplace = null;
				Error = null;
				CustomerMarketPlace = null;
			} // constructor
		} // class AddAccountState

		#endregion class AddAccountState

		#region method ValidateMpUniqueness

		private AddAccountState ValidateMpUniqueness(AccountModel model) {
			var oResult = new AddAccountState();

			try {
				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				m_oUniquenessChecker.Check(oResult.Marketplace.InternalId, m_oContext.Customer, model.Fill().UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException) {
				oResult.Error = CreateError(DbStrings.StoreAddedByYou);
			}
			catch (MarketPlaceIsAlreadyAddedException) {
				oResult.Error = CreateError(DbStrings.StoreAlreadyExistsInDb);
			}
			catch (Exception e) {
				ms_oLog.Error(e);
				oResult.Error = CreateError(e);
			} // try

			return oResult;
		} // ValidateMpUniqueness

		#endregion method ValidateMpUniqueness

		#region method SaveMarketplace

		private void SaveMarketplace(AddAccountState oState, AccountModel model) {
			try {
				model.id = m_oMpTypes.GetAll().First(a => a.InternalId == m_oVendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = m_oDatabaseHelper.SaveOrUpdateEncryptedCustomerMarketplace(
					model.name,
					oState.Marketplace,
					model,
					m_oContext.Customer
				);
				m_oSession.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				ms_oLog.Error(e);
				oState.Error = CreateError(e);
			} // try

			if (oState.Error != null)
				return;

			try {
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				m_oServiceClient.Instance.UpdateMarketplace(m_oContext.Customer.Id, oState.CustomerMarketPlace.Id, true);
			}
			catch (Exception e) {
				ms_oLog.Warn(e,
					"Failed to start UpdateMarketplace strategy for customer [{0}: {1}] with marketplace id {2}," +
					" if this is the only customer marketplace underwriter should run this strategy manually" +
					" (otherwise Main strategy will be stuck).",
					m_oContext.Customer.Id,
					m_oContext.Customer.Name,
					oState.CustomerMarketPlace.Id
				);
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

		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly MarketPlaceRepository m_oMpTypes;
		private readonly CGMPUniqChecker m_oUniquenessChecker;
		private readonly DatabaseDataHelper m_oDatabaseHelper;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession m_oSession;
		private readonly VendorInfo m_oVendorInfo;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(HmrcController));
		private static VendorInfo ms_oVendorInfo;
		private static readonly object ms_oLockVendorInfo = new object();

		private static readonly object ms_oLockCreateMarketplace = new object();

		#endregion fields

		#endregion private
	} // class HmrcController
} // namespace
