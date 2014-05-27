namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Code.MpUniq;
	using Customer.Controllers;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using Newtonsoft.Json;
	using ServiceClientProxy;
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
		public ActionResult SaveNewManuallyEntered(string sData) {
			if (string.IsNullOrWhiteSpace(sData))
				return CreateError("No data received.");

			HmrcManualDataModel oData = null;

			try {
				oData = JsonConvert.DeserializeObject<HmrcManualDataModel>(sData);
			}
			catch (Exception e) {
				return CreateError("Failed to parse input data: " + e.Message);
			} // try

			if (!oData.IsValid())
				return CreateError(string.Join("\n", oData.Errors));

			AccountModel model;
			AddAccountState oState;
			CreateAccountModel(oData.CustomerID, out model, out oState);

			if (oState.Error != null)
				return oState.Error;

			string stateError;
			Hopper oSeeds = ThrashManualData(oData, out stateError);

			if (stateError != null)
				return CreateError(stateError);

			return DoSave(oState, model, oData.CustomerID, oSeeds);
		} // SaveNewManuallyEntered

		private Hopper ThrashManualData(HmrcManualDataModel oData, out string sStateError) {
			sStateError = null;

			var oLog = new SafeILog(Log);

			var oResult = new Hopper();

			foreach (HmrcManualOnePeriodDataModel oPeriod in oData.VatPeriods) {
				var smd = new SheafMetaData {
					BaseFileName = "entered.manually." + oPeriod.Period + ".txt",
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null,
				};

				var oSeeds = new VatReturnSeeds();

				oSeeds.Set(VatReturnSeeds.Field.Period, oPeriod.Period, oLog);
				oSeeds.Set(VatReturnSeeds.Field.DateFrom, oPeriod.FromDate, oLog);
				oSeeds.Set(VatReturnSeeds.Field.DateTo, oPeriod.ToDate, oLog);
				oSeeds.Set(VatReturnSeeds.Field.DateDue, oPeriod.DueDate, oLog);

				if (!oSeeds.IsPeriodValid()) {
					sStateError = "Invalid period detected.";
					return null;
				} // if

				if (oData.RegNo > 0)
					oSeeds.Set(VatReturnSeeds.Field.RegistrationNo, oData.RegNo, oLog);

				oSeeds.Set(VatReturnSeeds.Field.BusinessName, oData.BusinessName, oLog);
				oSeeds.Set(VatReturnSeeds.Field.BusinessAddress, oData.BusinessAddress.Split('\n'), oLog);

				if (!oSeeds.AreBusinessDetailsValid()) {
					sStateError = "Invalid business details detected.";
					return null;
				} // if

				foreach (KeyValuePair<int, decimal> pair in oPeriod.BoxData) {
					int nBoxNum = pair.Key;
					decimal nAmount = pair.Value;

					string sFieldName = oData.BoxNames.ContainsKey(nBoxNum) ? oData.BoxNames[nBoxNum] : " (Box " + nBoxNum + ")";

					oSeeds.ReturnDetails[sFieldName] = new Coin(nAmount, "GBP");

					oLog.Debug("VatReturnSeeds.ReturnDetails[{0}] = {1}", sFieldName, nAmount);
				} // for each box

				oResult.Add(smd, oSeeds);
			} // for each period

			return oResult;
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
				m_oServiceClient.Instance.MarketplaceInstantUpdate(oState.CustomerMarketPlace.Id);
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e) {
				return CreateError("Account has been linked but error occurred while storing uploaded data: " + e.Message);
			} // try

			HmrcFileCache.Clean(Session, string.Format("HmrcFileCache{0}", nCustomerID));
			return Json(new { success = true, });
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
				stateError = oFileCache.DateIntervals.SortAndCheckSequence();
				return string.IsNullOrWhiteSpace(stateError) ? oFileCache.Hopper : null;
			} // switch
		} // GetProcessedFiles

		private void SaveMarketplace(AddAccountState oState, AccountModel model, Customer customer) {
			try {
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateEncryptedCustomerMarketplace(
					model.name,
					oState.Marketplace,
					model,
					customer
				);
				_session.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e.Message);
			} // try
		} // SaveMarketplace

		private JsonResult CreateError(string sErrorMsg) {
			return Json(new { success = string.IsNullOrWhiteSpace(sErrorMsg), error = sErrorMsg, });
		} // CreateError

		#endregion
	} // class UploadHmrcController
} // namespace
