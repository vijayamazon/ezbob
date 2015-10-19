namespace EzBob.Web.Infrastructure.Hmrc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using Areas.Customer.Controllers;
	using Code.MpUniq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using Models.Strings;
	using NHibernate;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using Coin = Ezbob.HmrcHarvester.Coin;

	public class HmrcManualAccountManager {
		public static JsonResult CreateJsonNoError() {
			return CreateJsonError(null);
		} // CreateJsonNoError

		public static JsonResult CreateJsonError(string sFormat, params object[] args) {
			string sErrorMessage = string.IsNullOrWhiteSpace(sFormat) ? null : string.Format(sFormat, args);

			bool bSuccess = string.IsNullOrWhiteSpace(sErrorMessage);

			if (!bSuccess)
				log.Warn("Returning error from HMRC manual account manager to web UI: {0}", sErrorMessage);

			return new JsonResult {
				Data = new { success = bSuccess, error = sErrorMessage, },
				ContentType = null,
				ContentEncoding = null,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet,
			};
		} // CreateJsonError

		public static JsonResult CreateJson(VatReturnPeriod[] oPeriods) {
			return new JsonResult {
				Data = new { aaData = oPeriods, },
				ContentType = null,
				ContentEncoding = null,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet,
			};
		} // CreateJsonError

		public HmrcManualAccountManager(
			CustomerRepository customers,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session, 
			IWorkplaceContext context
		) {
			if (hmrcVendorInfo == null) {
				lock (lockVendorInfo) {
					if (hmrcVendorInfo == null)
						hmrcVendorInfo = Configuration.Instance.GetVendorInfo("HMRC");
				} // lock
			} // if

			lock (lockVendorInfo) {
				this.vendorInfo = hmrcVendorInfo;
			} // lock

			this.customers = customers;
			this.databaseHelper = helper;
			this.mpTypes = mpTypes;
			this.uniquenessChecker = mpChecker;
			this.session = session;
			this.context = context;

			this.serviceClient = new ServiceClient();
		} // constructor

		public JsonResult SaveUploadedFiles(
			HttpFileCollectionBase oFiles,
			int nCustomerID,
			string sControllerName,
			string sActionName
		) {
			Customer oCustomer = this.customers.ReallyTryGet(nCustomerID);

			if (oCustomer == null)
				return CreateJsonError("Could not retrieve customer by id {0}.", nCustomerID);

			var oProcessor = new HmrcFileProcessor(nCustomerID, oFiles, sControllerName, sActionName);
			oProcessor.Run();

			if (!string.IsNullOrWhiteSpace(oProcessor.ErrorMsg))
				return CreateJsonError(oProcessor.ErrorMsg);

			if (oProcessor.AddedCount < 1)
				return CreateJsonError("No files were accepted.");

			return DoSave(
				oProcessor.Hopper,
				string.Format("Failed to upload VAT return file{0}.", oProcessor.AddedCount == 1 ? "" : "s"),
				oCustomer
			);
		} // SaveUploadedFiles

		public JsonResult SaveNewManuallyEntered(string sData) {
			if (string.IsNullOrWhiteSpace(sData))
				return CreateJsonError("No data received.");

			HmrcManualDataModel oData;

			try {
				oData = JsonConvert.DeserializeObject<HmrcManualDataModel>(sData);
			} catch (Exception e) {
				return CreateJsonError("Failed to parse input data: " + e.Message);
			} // try

			if (!oData.IsValid())
				return CreateJsonError(string.Join("\n", oData.Errors));

			Customer oCustomer = this.customers.ReallyTryGet(oData.CustomerID);

			if (oCustomer == null)
				return CreateJsonError("Could not retrieve customer by id {0}.", oData.CustomerID);

			string stateError;
			Hopper oSeeds = ThrashManualData(oData, out stateError);

			if (stateError != null)
				return CreateJsonError(stateError);

			return DoSave(oSeeds, "Failed to save manual VAT return data.", oCustomer);
		} // SaveNewManuallyEntered

		public JsonResult LoadPeriods(int nCustomerID) {
			try {
				VatReturnPeriodsActionResult vrpar = this.serviceClient.Instance.LoadManualVatReturnPeriods(nCustomerID);
				return CreateJson(vrpar.Periods);
			} catch (Exception e) {
				log.Warn(e, "Failed to load manual VAT return periods for customer {0}.", nCustomerID);
				return CreateJson(new VatReturnPeriod[0]);
			} // try
		} // LoadPeriods

		public JsonResult RemovePeriod(string sPeriod) {
			try {
				this.serviceClient.Instance.RemoveManualVatReturnPeriod(Guid.Parse(sPeriod));
				return CreateJsonNoError();
			} catch (Exception e) {
				log.Warn(e, "Failed to remove manual VAT return period for period id '{0}'.", sPeriod);
				return CreateJsonError("Failed to remove period.");
			} // try
		} // RemovePeriod

		private Hopper ThrashManualData(HmrcManualDataModel oData, out string sStateError) {
			sStateError = null;

			var oResult = new Hopper(VatReturnSourceType.Manual);

			foreach (HmrcManualOnePeriodDataModel oPeriod in oData.VatPeriods) {
				var smd = new SheafMetaData {
					BaseFileName = "entered.manually." + oPeriod.Period + ".txt",
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null,
				};

				var oSeeds = new VatReturnSeeds(log);

				oSeeds.Set(VatReturnSeeds.Field.Period, oPeriod.Period, log);
				oSeeds.Set(VatReturnSeeds.Field.DateFrom, oPeriod.FromDate, log);
				oSeeds.Set(VatReturnSeeds.Field.DateTo, oPeriod.ToDate, log);
				oSeeds.Set(VatReturnSeeds.Field.DateDue, oPeriod.DueDate, log);

				if (!oSeeds.IsPeriodValid()) {
					sStateError = "Invalid period detected: " + oSeeds.FatalError;
					return null;
				} // if

				if (oData.RegNo > 0)
					oSeeds.Set(VatReturnSeeds.Field.RegistrationNo, oData.RegNo, log);

				oSeeds.Set(VatReturnSeeds.Field.BusinessName, oData.BusinessName, log);
				oSeeds.Set(VatReturnSeeds.Field.BusinessAddress, oData.BusinessAddress.Split('\n'), log);

				if (!oSeeds.AreBusinessDetailsValid()) {
					sStateError = "Invalid business details detected: " + oSeeds.FatalError;
					return null;
				} // if

				foreach (KeyValuePair<int, decimal> pair in oPeriod.BoxData) {
					int nBoxNum = pair.Key;
					decimal nAmount = pair.Value;

					string sFieldName = oData.BoxNames.ContainsKey(nBoxNum)
						? oData.BoxNames[nBoxNum]
						: " (Box " + nBoxNum + ")";

					oSeeds.ReturnDetails[sFieldName] = new Coin(nAmount, "GBP");

					log.Debug("VatReturnSeeds.ReturnDetails[{0}] = {1}", sFieldName, nAmount);
				} // for each box

				oResult.Add(smd, oSeeds);
			} // for each period

			return oResult;
		} // ThrashManualData

		private JsonResult DoSave(Hopper oHopper, string sNoAccountError, Customer oCustomer) {
			IDatabaseCustomerMarketPlace mp = FindOrCreateMarketplace(oCustomer);

			if (mp == null) {
				log.Alert("Marketplace neither found nor created.");
				return CreateJsonError(sNoAccountError);
			} // if

			Connector.SetBackdoorData(this.vendorInfo.Name, mp.Id, oHopper);

			try {
				this.serviceClient.Instance.MarketplaceInstantUpdate(mp.Id);
				mp.Marketplace.GetRetrieveDataHelper(this.databaseHelper).CustomerMarketplaceUpdateAction(mp.Id);
			} catch (Exception e) {
				return CreateJsonError("Account has been linked but error occurred while storing the data: " + e.Message);
			} // try

			return CreateJsonNoError();
		} // DoSave

		private IDatabaseCustomerMarketPlace FindMarketplace(Customer oCustomer) {
			MP_CustomerMarketPlace oMp = oCustomer.CustomerMarketPlaces.FirstOrDefault(mp => {
				if (mp.Marketplace.InternalId != this.vendorInfo.Guid())
					return false;

				if (mp.DisplayName != oCustomer.Name)
					return false;

				return AccountModel.ToModel(mp).password == VendorInfo.TopSecret;
			});

			if (oMp != null) {
				oMp.SetIMarketplaceType(new DatabaseMarketPlace(this.vendorInfo.Name));
				return oMp;
			} // if

			return null;
		} // FindMarketplace

		private IDatabaseCustomerMarketPlace FindOrCreateMarketplace(Customer oCustomer) {
			IDatabaseCustomerMarketPlace oMp = FindMarketplace(oCustomer);

			if (oMp != null)
				return oMp;

			lock (lockCreateMarketplace) {
				oMp = FindMarketplace(oCustomer);

				if (oMp != null)
					return oMp;

				var model = new AccountModel {
					accountTypeName = this.vendorInfo.Name,
					displayName = oCustomer.Name,
					name = oCustomer.Name,
					login = oCustomer.Name,
					password = VendorInfo.TopSecret,
				};

				AddAccountState oState = ValidateMpUniqueness(model, oCustomer);

				if (oState.Error != null)
					return null;

				SaveMarketplace(oState, model, oCustomer);

				return oState.CustomerMarketPlace;
			} // lock
		} // FindOrCreateMarketplace

		private class AddAccountState {
			public IMarketplaceType Marketplace;
			public HmrcManualAccountManagerException Error;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState() {
				Marketplace = null;
				Error = null;
				CustomerMarketPlace = null;
			} // constructor
		} // class AddAccountState

		private AddAccountState ValidateMpUniqueness(AccountModel model, Customer oCustomer) {
			var oResult = new AddAccountState();

			try {
				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				this.uniquenessChecker.Check(oResult.Marketplace.InternalId, oCustomer, model.Fill().UniqueID());
			} catch (MarketPlaceAddedByThisCustomerException) {
				oResult.Error = new HmrcManualAccountManagerException(DbStrings.StoreAddedByYou);
			} catch (MarketPlaceIsAlreadyAddedException) {
				oResult.Error = new HmrcManualAccountManagerException(DbStrings.StoreAlreadyExistsInDb);
			} catch (Exception e) {
				log.Error(e);
				oResult.Error = new HmrcManualAccountManagerException(e);
			} // try

			return oResult;
		} // ValidateMpUniqueness

		private void SaveMarketplace(AddAccountState oState, AccountModel model, Customer oCustomer) {
			try {
				model.id = this.mpTypes.GetAll().First(a => a.InternalId == this.vendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = this.databaseHelper.SaveOrUpdateEncryptedCustomerMarketplace(
					model.name,
					oState.Marketplace,
					model,
					oCustomer
				);
				this.session.Flush();

				oState.CustomerMarketPlace = mp;
			} catch (Exception e) {
				log.Error(e);
				oState.Error = new HmrcManualAccountManagerException(e);
			} // try

			if (oState.Error != null)
				return;

			try {
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				this.serviceClient.Instance.UpdateMarketplace(
					oCustomer.Id,
					oState.CustomerMarketPlace.Id,
					true,
					this.context.UserId
				);
			}
			catch (Exception e) {
				log.Warn(e,
					"Failed to start UpdateMarketplace strategy for customer [{0}: {1}] with marketplace id {2}," +
					" if this is the only customer marketplace underwriter should run this strategy manually" +
					" (otherwise Main strategy will be stuck).",
					oCustomer.Id,
					oCustomer.Name,
					oState.CustomerMarketPlace.Id
				);
			} // try
		} // SaveMarketplace

		private readonly MarketPlaceRepository mpTypes;
		private readonly CGMPUniqChecker uniquenessChecker;
		private readonly DatabaseDataHelper databaseHelper;
		private readonly ServiceClient serviceClient;
		private readonly ISession session;
		private readonly VendorInfo vendorInfo;
		private readonly CustomerRepository customers;
		private readonly IWorkplaceContext context;

		private static volatile VendorInfo hmrcVendorInfo;
		private static readonly object lockVendorInfo = new object();
		private static readonly object lockCreateMarketplace = new object();

		private static readonly ASafeLog log = new SafeILog(typeof(HmrcManualAccountManager));
	} // class HmrcManualAccountManager
} // namespace
