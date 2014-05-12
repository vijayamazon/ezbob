namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using Code.MpUniq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.HmrcHarvester;
	using Ezbob.ValueIntervals;
	using Infrastructure;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using ServiceClientProxy;
	using log4net;
	using NHibernate;
	using Web.Models.Strings;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class HmrcController : Controller {
		#region public

		#region class ValidateFilesResult

		public class ValidateFilesResult {
			public Hopper Hopper;
			public string Error;
		} // class ValidateFilesResult

		#endregion class ValidateFilesResult

		#region method ValidateFiles

		public static ValidateFilesResult ValidateFiles(int nCustomerID, HttpFileCollectionBase oFiles) {
			var oProcessor = new LocalHmrcFileProcessor(nCustomerID, oFiles);

			oProcessor.Run();

			return oProcessor;
		} // ValidateFiles

		#endregion method ValidateFiles

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

		#region action UploadFiles

		[HttpPost]
		public ActionResult UploadFiles() {
			HmrcFileCache oFileCache = HmrcFileCache.Get(Session);

			if ((oFileCache != null) && !string.IsNullOrWhiteSpace(oFileCache.ErrorMsg))
				return CreateError(oFileCache.ErrorMsg);

			string customerEmail = _context.Customer.Name;
			var model = new AccountModel { accountTypeName = "HMRC", displayName = customerEmail, name = customerEmail, login = customerEmail, password = "topsecret" };

			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null)
				return oState.Error;

			string stateError;
			Hopper oSeeds = GetProcessedFiles(out stateError);

			if (stateError != null)
				return CreateError(stateError);

			SaveMarketplace(oState, model);

			if (oState.Error != null)
				return oState.Error;

			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try {
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, oState.CustomerMarketPlace.Id, true);
			}
			catch (Exception e) {
				Log.WarnFormat(
					"Failed to start UpdateMarketplace strategy for customer [{0}: {1}] with marketplace id {2}," +
					" if this is the only customer marketplace underwriter should run this strategy manually" +
					" (otherwise Main strategy will be stuck).",
					_context.Customer.Id,
					_context.Customer.Name,
					oState.CustomerMarketPlace.Id
				);
				Log.Warn(e);
			} // try

			try {
				m_oServiceClient.Instance.MarketplaceInstantUpdate(oState.CustomerMarketPlace.Id);
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e) {
				return CreateError("Account has been linked but error occurred while storing uploaded data: " + e.Message);
			} // try

			return Json(new { });
		} // UploadFiles

		#endregion action UploadFiles

		#region action UploadedFiles

		[HttpPost]
		public ActionResult UploadedFiles() {
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			int nCustomerID = 0;

			try {
				nCustomerID = _context.Customer.Id;
			}
			catch (Exception e) {
				Log.Warn("Failed to fetch current customer, files will be saved without customer ID; exception: ", e);
			} // try

			var oProcessor = new SessionHmrcFileProcessor(Session, nCustomerID, Request.Files);

			oProcessor.Run();

			if (!string.IsNullOrWhiteSpace(oProcessor.FileCache.ErrorMsg))
				return CreateError(oProcessor.FileCache.ErrorMsg);

			return Json(new { });
		} // UploadedFiles

		#endregion action UploadedFiles

		#endregion public

		#region private

		#region method GetProcessedFiles

		private Hopper GetProcessedFiles(out string stateError) {
			stateError = null;

			HmrcFileCache oFileCache = HmrcFileCache.Get(Session);

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
						stateError = "Inconsequent date ranges: " + prev + " and " + next;
						return null;
					} // if
				} // for each interval

				return oFileCache.Hopper;
			} // switch
		} // GetProcessedFiles

		#endregion method GetProcessedFiles

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
				Log.Error(sError);
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
				Log.Error(e);
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

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(model.name, oState.Marketplace, model, _context.Customer);
				_session.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e);
			} // try
		} // SaveMarketplace

		#endregion method SaveMarketplace

		#region method CreateError

		private JsonResult CreateError(Exception ex) {
			return CreateError(ex.Message);
		} // CreateError

		private JsonResult CreateError(string sErrorMsg) {
			Log.WarnFormat("Returning error from HMRC controller to web UI: {0}", sErrorMsg);
			return Json(new { error = sErrorMsg });
		} // CreateError

		#endregion method CreateError

		#region properties

		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(HmrcController));

		#endregion properties

		#endregion private
	} // class HmrcController
} // namespace
