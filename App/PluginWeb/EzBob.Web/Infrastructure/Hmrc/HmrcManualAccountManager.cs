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
	using Ezbob.Database;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using Models.Strings;
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
			bool hideRealError,
			CustomerRepository customers,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
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

			this.hideRealError = hideRealError;
			this.customers = customers;
			this.databaseHelper = helper;
			this.mpTypes = mpTypes;
			this.uniquenessChecker = mpChecker;
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

			if (oCustomer == null) {
				if (this.hideRealError) {
					log.Warn("Could not retrieve customer by id {0}.", nCustomerID);
					return CreateJsonError("Could not retrieve customer by requested id.");
				} // if

				return CreateJsonError("Could not retrieve customer by id {0}.", nCustomerID);
			} // if

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

		private static Hopper ThrashManualData(HmrcManualDataModel oData, out string sStateError) {
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
			int mpID = FindOrCreateMarketplace(oCustomer);

			if (mpID <= 0) {
				log.Alert("Marketplace neither found nor created for customer {0}.", oCustomer.Stringify());
				return CreateJsonError(sNoAccountError);
			} // if

			Connector.SetBackdoorData(this.vendorInfo.Name, mpID, oHopper);

			try {
				this.serviceClient.Instance.MarketplaceInstantUpdate(mpID);

				new RetrieveDataHelper(
					this.databaseHelper,
					new DatabaseMarketPlace(hmrcVendorInfo.Name)
				).CustomerMarketplaceUpdateAction(mpID);
			} catch (Exception e) {
				log.Error(e, "Account has been linked but error occurred while storing the data.");

				return CreateJsonError(this.hideRealError
					? "Internal error occurred while storing VAT return data."
					: "Account has been linked but error occurred while storing the data: " + e.Message
				);
			} // try

			return CreateJsonNoError();
		} // DoSave

		private int FindMarketplace(Customer oCustomer) {
			MP_CustomerMarketPlace oMp = oCustomer.CustomerMarketPlaces.FirstOrDefault(mp => {
				if (mp.Marketplace.InternalId != this.vendorInfo.Guid())
					return false;

				if (!mp.DisplayName.StartsWith(oCustomer.Name))
					return false;

				return AccountModel.ToModel(mp).password == VendorInfo.TopSecret;
			});

			return oMp != null ? oMp.Id : 0;
		} // FindMarketplace

		private int FindOrCreateMarketplace(Customer oCustomer) {
			log.Debug("FindOrCreateMarketplace for customer {0} started...", oCustomer.Stringify());

			int mpID = FindMarketplace(oCustomer);

			if (mpID > 0) {
				log.Debug(
					"FindOrCreateMarketplace for customer {0} complete: found {1} on the first check.",
					oCustomer.Stringify(),
					mpID
				);

				return mpID;
			} // if

			log.Debug(
				"FindOrCreateMarketplace for customer {0}: not found on the first check, locking...",
				oCustomer.Stringify()
			);

			lock (lockCreateMarketplace) {
				log.Debug(
					"FindOrCreateMarketplace for customer {0}: locked.",
					oCustomer.Stringify()
				);

				mpID = FindMarketplace(oCustomer);

				if (mpID > 0) {
					log.Debug(
						"FindOrCreateMarketplace for customer {0} complete: found {1} on the second check.",
						oCustomer.Stringify(),
						mpID
					);

					return mpID;
				} // if

				log.Debug(
					"FindOrCreateMarketplace for customer {0}: not found on the second check, creating model...",
					oCustomer.Stringify()
				);

				var model = new AccountModel {
					accountTypeName = this.vendorInfo.Name,
					displayName = oCustomer.Name,
					name = oCustomer.Name,
					login = oCustomer.Name,
					password = VendorInfo.TopSecret,
				};

				log.Debug(
					"FindOrCreateMarketplace for customer {0}: model is ready for uniqueness check.",
					oCustomer.Stringify()
				);

				AddAccountState oState = ValidateMpUniqueness(model, oCustomer);

				if (oState.Error != null) {
					log.Debug(
						"FindOrCreateMarketplace for customer {0} complete: model is not unique.",
						oCustomer.Stringify()
					);

					return 0;
				} // if

				log.Debug(
					"FindOrCreateMarketplace for customer {0}: creating a new marketplace.",
					oCustomer.Stringify()
				);

				SaveMarketplace(oState, model, oCustomer);

				log.Debug(
					"FindOrCreateMarketplace for customer {0} complete: created {1}.",
					oCustomer.Stringify(),
					oState.CustomerMarketPlaceID
				);

				return oState.CustomerMarketPlaceID;
			} // lock
		} // FindOrCreateMarketplace

		private class AddAccountState {
			public IMarketplaceType Marketplace;
			public HmrcManualAccountManagerException Error;
			public int CustomerMarketPlaceID;

			public AddAccountState() {
				Marketplace = null;
				Error = null;
				CustomerMarketPlaceID = 0;
			} // constructor
		} // class AddAccountState

		private AddAccountState ValidateMpUniqueness(AccountModel model, Customer oCustomer) {
			log.Debug("ValidateMpUniqueness started...");

			var oResult = new AddAccountState();

			log.Debug("ValidateMpUniqueness: result holder created.");

			try {
				log.Debug("ValidateMpUniqueness: going for it...");

				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);

				string uniqueID = model.Fill().UniqueID();

				log.Debug("ValidateMpUniqueness: result marketplace is set, unique id is '{0}'.", uniqueID);

				this.uniquenessChecker.Check(oResult.Marketplace.InternalId, oCustomer, uniqueID);

				log.Debug("ValidateMpUniqueness: congrats! It's unique.");
			} catch (MarketPlaceAddedByThisCustomerException) {
				log.Error("ValidateMpUniqueness: oops! The same customer already has this.");
				oResult.Error = new HmrcManualAccountManagerException(DbStrings.StoreAddedByYou);
			} catch (MarketPlaceIsAlreadyAddedException) {
				log.Error("ValidateMpUniqueness: oops! Someone else already has this.");
				oResult.Error = new HmrcManualAccountManagerException(DbStrings.StoreAlreadyExistsInDb);
			} catch (Exception e) {
				log.Error(e, "ValidateMpUniqueness: something went wrong while checking for uniqueness.");
				oResult.Error = new HmrcManualAccountManagerException(e);
			} // try

			log.Debug("ValidateMpUniqueness complete with{0} error.", oResult.Error == null ? "out" : string.Empty);

			return oResult;
		} // ValidateMpUniqueness

		private void SaveMarketplace(AddAccountState oState, AccountModel model, Customer oCustomer) {
			try {
				model.id = this.mpTypes.GetAll().First(a => a.InternalId == this.vendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				SafeReader sr = DbConnectionGenerator.Get(log).GetFirst(
					"CreateOrLoadUploadedHmrcMarketplace",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", oCustomer.Id),
					new QueryParameter("@SecurityData", new Encrypted(new Serialized(model))),
					new QueryParameter("@Now", DateTime.UtcNow)
				);

				int spExitCode = sr["ExitCode"];
				oState.CustomerMarketPlaceID = 0;

				switch (spExitCode) {
				case 0:
					oState.CustomerMarketPlaceID = sr["MpID"];
					log.Info(
						"Successfully created uploaded/manual HMRC marketplace {0} for customer {1}.",
						oState.CustomerMarketPlaceID,
						oCustomer.Stringify()
					);
					break;

				case -1:
					log.Alert(
						"Failed to create uploaded/manual HMRC marketplace for customer {0}.",
						oCustomer.Stringify()
					);
					break;

				case -2:
					log.Alert(
						"Uploaded/manual HMRC marketplace with email '{0}' that should belong to customer {1}" +
						"already exists with id {2} and belongs to customer {3}.",
						oCustomer.Name,
						oCustomer.Stringify(),
						sr["MpID"],
						sr["OtherCustomerID"]
					);
					break;

				default:
					log.Alert(
						"Other error while creating uploaded/manual HMRC marketplace for customer {0}.",
						oCustomer.Stringify()
					);
					break;
				} // switch

				if (spExitCode < 0)
					throw new Exception("Failed to save VAT return data.");
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
					oState.CustomerMarketPlaceID,
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
					oState.CustomerMarketPlaceID
				);
			} // try
		} // SaveMarketplace

		private readonly bool hideRealError;
		private readonly MarketPlaceRepository mpTypes;
		private readonly CGMPUniqChecker uniquenessChecker;
		private readonly DatabaseDataHelper databaseHelper;
		private readonly ServiceClient serviceClient;
		private readonly VendorInfo vendorInfo;
		private readonly CustomerRepository customers;
		private readonly IWorkplaceContext context;

		private static volatile VendorInfo hmrcVendorInfo;
		private static readonly object lockVendorInfo = new object();
		private static readonly object lockCreateMarketplace = new object();

		private static readonly ASafeLog log = new SafeILog(typeof(HmrcManualAccountManager));
	} // class HmrcManualAccountManager
} // namespace
