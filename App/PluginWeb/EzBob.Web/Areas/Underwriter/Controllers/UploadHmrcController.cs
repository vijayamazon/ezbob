namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using Code;
	using Code.MpUniq;
	using Customer.Controllers;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.HmrcHarvester;
	using Ezbob.ValueIntervals;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using Web.Models.Strings;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class UploadHmrcController : Controller {
		private static readonly ILog Log = LogManager.GetLogger(typeof(UploadHmrcController));
		private readonly CustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly ISession _session;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly DatabaseDataHelper _helper;

		public UploadHmrcController(
			CustomerRepository customers,
			CGMPUniqChecker mpChecker,
			MarketPlaceRepository mpTypes,
			DatabaseDataHelper helper,
			ISession session
		) {
			m_oServiceClient = new ServiceClient();
			_customers = customers;
			_mpChecker = mpChecker;
			_helper = helper;
			_mpTypes = mpTypes;
			_session = session;
		} // constructor

		#region HMRC Upload

		[HttpPost]
		public ActionResult UploadHmrc(int customerId) {
			if (customerId < 1)
				return Json(new { error = "Customer not specified" });

			var oProcessor = new SessionHmrcFileProcessor(Session, customerId, Request.Files, string.Format("HmrcFileCache{0}", customerId));

			oProcessor.Run();

			if (!string.IsNullOrWhiteSpace(oProcessor.FileCache.ErrorMsg))
				return Json(new { error = oProcessor.FileCache.ErrorMsg });

			return Json(new { });
		} // UploadHmrc

		[HttpPost]
		public ActionResult UploadFiles(int customerId) {
			HmrcFileCache oFileCache = HmrcFileCache.Get(Session, string.Format("HmrcFileCache{0}", customerId));

			if ((oFileCache != null) && !string.IsNullOrWhiteSpace(oFileCache.ErrorMsg))
				return CreateError(oFileCache.ErrorMsg);

			AccountModel model;
			AddAccountState oState;
			CreateAccountModel(customerId, out model, out oState);

			if (oState.Error != null)
				return oState.Error;

			string stateError;
			Hopper oSeeds = GetProcessedFiles(customerId, out stateError);

			if (stateError != null)
				return CreateError(stateError);

			return DoSave(oState, model, customerId, oSeeds);
		} // UploadFiles

		[HttpPost]
		public ActionResult SaveNewManuallyEntered(int nCustomerID, object data) {
			AccountModel model;
			AddAccountState oState;
			CreateAccountModel(nCustomerID, out model, out oState);

			if (oState.Error != null)
				return oState.Error;

			string stateError;
			Hopper oSeeds = ThrashManualData(nCustomerID, data, out stateError);

			if (stateError != null)
				return CreateError(stateError);

			return DoSave(oState, model, nCustomerID, oSeeds);
		} // SaveNewManuallyEntered

		private Hopper ThrashManualData(int nCustomerID, object data, out string sStateError) {
			sStateError = null;

			// TODO

			return null;
		} // ThrashManualData

		private void CreateAccountModel(int nCustomerID, out AccountModel model, out AddAccountState oState) {
			Customer customer = _customers.Get(nCustomerID);

			string customerEmail = customer.Name;

			model = new AccountModel {
				accountTypeName = "HMRC",
				displayName = customerEmail,
				name = customerEmail,
				login = customerEmail,
				password = "topsecret"
			};

			oState = new AddAccountState { VendorInfo = Configuration.Instance.GetVendorInfo(model.accountTypeName) };

			if (oState.VendorInfo == null) {
				var sError = "Unsupported account type: " + model.accountTypeName;
				Log.Error(sError);
				oState.Error = CreateError(sError);
				return;
			} // try

			try {
				oState.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				_mpChecker.Check(oState.Marketplace.InternalId, customer, model.Fill().UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException) {
				oState.Error = CreateError(DbStrings.StoreAddedByYou);
			}
			catch (MarketPlaceIsAlreadyAddedException) {
				oState.Error = CreateError(DbStrings.StoreAlreadyExistsInDb);
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e.Message);
			} // try
		} // CreateAccountModel

		private ActionResult DoSave(AddAccountState oState, AccountModel model, int nCustomerID, Hopper oSeeds) {
			Customer customer = _customers.Get(nCustomerID);

			SaveMarketplace(oState, model, customer);

			if (oState.Error != null)
				return oState.Error;

			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try {
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				m_oServiceClient.Instance.UpdateMarketplace(customer.Id, oState.CustomerMarketPlace.Id, true);
			}
			catch (Exception e) {
				Log.WarnFormat(
					"Failed to start UpdateMarketplace strategy for customer [{0}: {1}] with marketplace id {2}," +
					" if this is the only customer marketplace underwriter should run this strategy manually" +
					" (otherwise Main strategy will be stuck).",
					customer.Id,
					customer.Name,
					oState.CustomerMarketPlace.Id
				);
				Log.Warn(e);
			} // try

			try {
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e) {
				return CreateError("Account has been linked but error occurred while storing uploaded data: " + e.Message);
			} // try

			HmrcFileCache.Clean(Session, string.Format("HmrcFileCache{0}", nCustomerID));
			return Json(new { });
		} // DoSave

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

		private Hopper GetProcessedFiles(int customerId, out string stateError) {
			stateError = null;

			HmrcFileCache oFileCache = HmrcFileCache.Get(Session, string.Format("HmrcFileCache{0}", customerId));

			if (oFileCache == null) {
				stateError = "No files were successfully processed";
				return null;
			} // if

			switch (oFileCache.AddedCount) {
			case 0:
				stateError = "No files were successfully processed";
				return null;

			case 1:
				return oFileCache.Hopper;

			default:
				oFileCache.DateIntervals.Sort((a, b) => a.Left.CompareTo(b.Left));

				DateInterval next = null;

				foreach (DateInterval cur in oFileCache.DateIntervals) {
					if (next == null) {
						next = cur;
						continue;
					} // if

					DateInterval prev = next;
					next = cur;

					if (!prev.IsJustBefore(next)) {
						stateError = "In-consequent date ranges: " + prev + " and " + next;
						return null;
					} // if
				} // for each interval

				return oFileCache.Hopper;
			} // switch
		} // GetProcessedFiles

		private void SaveMarketplace(AddAccountState oState, AccountModel model, Customer customer) {
			try {
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(model.name, oState.Marketplace, model, customer);
				_session.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e.Message);
			} // try
		} // SaveMarketplace

		private JsonResult CreateError(string sErrorMsg) {
			return Json(new { error = sErrorMsg });
		} // CreateError

		#endregion
	} // class UploadHmrcController
} // namespace
