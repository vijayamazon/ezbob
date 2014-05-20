namespace EzBob.Web.Areas.Customer.Controllers {
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using NHibernate;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using Infrastructure;
	using Code.MpUniq;
	using ServiceClientProxy;
	using Web.Models.Strings;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using Newtonsoft.Json;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CGMarketPlacesController : Controller {
		#region public

		#region constructor

		public CGMarketPlacesController(
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

		#region method UploadFilesDialog

		public ActionResult UploadFilesDialog(string key, string handler, string modelkey) {
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			ViewData["key"] = key;
			ViewData["handler"] = handler;
			ViewData["modelkey"] = modelkey;

			return View();
		} // UploadFilesDialog

		#endregion method UploadFilesDialog

		#region method HandleUploadedHmrcVatReturn

		[HttpPost]
		public ActionResult HandleUploadedHmrcVatReturn() {
			Response.AddHeader("x-frame-options", "SAMEORIGIN");
			ViewData["key"] = Request["key"];
			
			var customerEmail = _context.Customer.Name;
			var model = new AccountModel { accountTypeName = "HMRC", displayName = customerEmail, name = customerEmail, login = customerEmail, password = "topsecret" };
			
			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null) {
				ViewError = oState.Error;
				ViewModel = null;
				return View();
			} // if

			int nCustomerID = 0;

			try {
				nCustomerID = _context.Customer.Id;
			}
			catch (Exception e) {
				Log.Warn("Failed to fetch current customer, files will be saved without customer ID; exception: ", e);
			} // try

			HmrcController.ValidateFilesResult oValidateResult = HmrcController.ValidateFiles(nCustomerID, Request.Files);

			if (!string.IsNullOrWhiteSpace(oValidateResult.Error))
				oState.Error = CreateError(oValidateResult.Error);

			if (oState.Error != null) {
				ViewError = oState.Error;
				ViewModel = null;
				return View();
			} // if

			if (oValidateResult.Hopper == null) {
				ViewError = CreateError("No files accepted.");
				ViewModel = null;
				return View();
			} // if

			SaveMarketplace(oState, model);

			if (oState.Error != null) {
				ViewError = oState.Error;
				ViewModel = null;
				return View();
			} // if

			ViewModel = JsonConvert.SerializeObject(oState.Model);
			ViewError = null;

			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oValidateResult.Hopper);

			try {
				m_oServiceClient.Instance.MarketplaceInstantUpdate(oState.CustomerMarketPlace.Id);
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e) {
				ViewError = CreateError("Account has been linked but error occurred while storing uploaded data: " + e.Message);
			} // try

			try {
				// This is done to insert entries into EzServiceActionHistory
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

			return View();
		} // HandleUploadedHmrcVatReturn

		#endregion method HandleUploadedHmrcVatReturn

		#region method Accounts (account list by type)

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

		#endregion method Accounts (account list by type)

		#region method Accounts (add new account)

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

			if (mpId != -1)
				m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, mpId, true);

			if (oState.Error != null)
				return oState.Error;

			return oState.Model;
		} // Accounts

		#endregion method Accounts (add new account)

		#endregion public

		#region private

		#region class AddAccountState

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

		#endregion class AddAccountState

		#region method ValidateModel

		private AddAccountState ValidateModel(AccountModel model) {
			var oResult = new AddAccountState();

			oResult.VendorInfo = Configuration.Instance.GetVendorInfo(model.accountTypeName);

			if (oResult.VendorInfo == null) {
				var sError = "Unsupported account type: " + model.accountTypeName;
				Log.Error(sError);
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
				Log.Error(e);
				oResult.Error = CreateError(e);
				return oResult;
			} // try

			return oResult;
		} // ValidateModel

		#endregion method ValidateModel

		#region method ValidateAccount

		private void ValidateAccount(AddAccountState oState, AccountModel model) {
			try {
				var ctr = new Connector(oState.AccountData, Log, _context.Customer);

				if (ctr.Init()) {
					ctr.Run(true);
					ctr.Done();
				} // if
			}
			catch (ConnectionFailException cge) {
				if (DBConfigurationValues.Instance.ChannelGrabberRejectPolicy == ChannelGrabberRejectPolicy.ConnectionFail) {
					Log.Error(cge);
					oState.Error = CreateError(cge);
				} // if

				Log.ErrorFormat("Failed to validate {0} account, continuing with registration.", model.accountTypeName);
				Log.Error(cge);

				// Error is logged but not written into state.
			}
			catch (ApiException cge) {
				Log.ErrorFormat("Failed to validate {0} account.", model.accountTypeName);
				Log.Error(cge);
				oState.Error = CreateError(cge);
			}
			catch (InvalidCredentialsException ice) {
				Log.Info(ice);
				oState.Error = CreateError(ice);
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e);
			} // try
		} // ValidateAccount

		#endregion method ValidateAccount

		#region method SaveMarketplace

		[NonAction]
		[Transactional]
		private int SaveMarketplace(AddAccountState oState, AccountModel model) {
			try {
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
				return mp.Id;
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e);
				return -1;
			} // try
		} // SaveMarketplace

		#endregion method SaveMarketplace
		
		#region method CreateError

		private JsonResult CreateError(Exception ex) {
			return CreateError(ex.Message);
		} // CreateError

		private JsonResult CreateError(string sErrorMsg) {
			return Json(new { error = sErrorMsg }, JsonRequestBehavior.AllowGet);
		} // CreateError

		#endregion method CreateError

		#region property ViewError

		private JsonResult ViewError { set {
			ViewData["error"] = JsonConvert.SerializeObject(value);
		} } // ViewError

		#endregion property ViewError

		#region property ViewModel

		private string ViewModel { set { ViewData["model"] = value ?? "null"; } } // ViewModel

		#endregion property ViewModel

		#region fields

		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly ServiceClient m_oServiceClient;
		private readonly DatabaseDataHelper _helper;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(CGMarketPlacesController));

		#endregion fields

		#endregion private
	} // class CGMarketPlacesController
} // namespace