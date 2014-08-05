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
		#region public

		#region static method CreateJsonNoError

		public static JsonResult CreateJsonNoError() {
			return CreateJsonError(null);
		} // CreateJsonNoError

		#endregion static method CreateJsonNoError

		#region static method CreateJsonError

		public static JsonResult CreateJsonError(string sFormat, params object[] args) {
			string sErrorMessage = string.IsNullOrWhiteSpace(sFormat) ? null : string.Format(sFormat, args);

			bool bSuccess = string.IsNullOrWhiteSpace(sErrorMessage);

			if (!bSuccess)
				ms_oLog.Warn("Returning error from HMRC manual account manager to web UI: {0}", sErrorMessage);

			return new JsonResult {
				Data = new { success = bSuccess, error = sErrorMessage, },
				ContentType = null,
				ContentEncoding = null,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet,
			};
		} // CreateJsonError

		#endregion static method CreateJsonError

		#region static method CreateJson

		public static JsonResult CreateJson(VatReturnPeriod[] oPeriods) {
			return new JsonResult {
				Data = new { aaData = oPeriods, },
				ContentType = null,
				ContentEncoding = null,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet,
			};
		} // CreateJsonError

		#endregion static method CreateJson

		#region constructor

		public HmrcManualAccountManager(
			CustomerRepository customers,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session
		) {
			if (ms_oVendorInfo == null) {
				lock (ms_oLockVendorInfo) {
					if (ms_oVendorInfo == null)
						ms_oVendorInfo = Configuration.Instance.GetVendorInfo("HMRC");
				} // lock
			} // if

			lock (ms_oLockVendorInfo) {
				m_oVendorInfo = ms_oVendorInfo;
			} // lock

			m_oCustomers = customers;
			m_oDatabaseHelper = helper;
			m_oMpTypes = mpTypes;
			m_oUniquenessChecker = mpChecker;
			m_oSession = session;

			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region method SaveUploadedFiles

		public JsonResult SaveUploadedFiles(HttpFileCollectionBase oFiles, int nCustomerID) {
			Customer oCustomer = m_oCustomers.ReallyTryGet(nCustomerID);

			if (oCustomer == null)
				return CreateJsonError("Could not retrieve customer by id {0}.", nCustomerID);

			var oProcessor = new HmrcFileProcessor(nCustomerID, oFiles);
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

		#endregion method SaveUploadedFiles

		#region method SaveNewManuallyEntered

		public JsonResult SaveNewManuallyEntered(string sData) {
			if (string.IsNullOrWhiteSpace(sData))
				return CreateJsonError("No data received.");

			HmrcManualDataModel oData;

			try {
				oData = JsonConvert.DeserializeObject<HmrcManualDataModel>(sData);
			}
			catch (Exception e) {
				return CreateJsonError("Failed to parse input data: " + e.Message);
			} // try

			if (!oData.IsValid())
				return CreateJsonError(string.Join("\n", oData.Errors));

			Customer oCustomer = m_oCustomers.ReallyTryGet(oData.CustomerID);

			if (oCustomer == null)
				return CreateJsonError("Could not retrieve customer by id {0}.", oData.CustomerID);

			string stateError;
			Hopper oSeeds = ThrashManualData(oData, out stateError);

			if (stateError != null)
				return CreateJsonError(stateError);

			return DoSave(oSeeds, "Failed to save manual VAT return data.", oCustomer);
		} // SaveNewManuallyEntered

		#endregion method SaveNewManuallyEntered

		#region method LoadPeriods

		public JsonResult LoadPeriods(int nCustomerID) {
			try {
				VatReturnPeriodsActionResult vrpar = m_oServiceClient.Instance.LoadManualVatReturnPeriods(nCustomerID);
				return CreateJson(vrpar.Periods);
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to load manual VAT return periods for customer {0}.", nCustomerID);
				return CreateJson(new VatReturnPeriod[0]);
			} // try
		} // LoadPeriods

		#endregion method LoadPeriods

		#region method RemovePeriod

		public JsonResult RemovePeriod(string sPeriod) {
			try {
				m_oServiceClient.Instance.RemoveManualVatReturnPeriod(Guid.Parse(sPeriod));
				return CreateJsonNoError();
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to remove manual VAT return period for period id '{0}'.", sPeriod);
				return CreateJsonError("Failed to remove period.");
			} // try
		} // RemovePeriod

		#endregion method RemovePeriod

		#endregion public

		#region private

		#region method ThrashManualData

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

				var oSeeds = new VatReturnSeeds(ms_oLog);

				oSeeds.Set(VatReturnSeeds.Field.Period, oPeriod.Period, ms_oLog);
				oSeeds.Set(VatReturnSeeds.Field.DateFrom, oPeriod.FromDate, ms_oLog);
				oSeeds.Set(VatReturnSeeds.Field.DateTo, oPeriod.ToDate, ms_oLog);
				oSeeds.Set(VatReturnSeeds.Field.DateDue, oPeriod.DueDate, ms_oLog);

				if (!oSeeds.IsPeriodValid()) {
					sStateError = "Invalid period detected: " + oSeeds.FatalError;
					return null;
				} // if

				if (oData.RegNo > 0)
					oSeeds.Set(VatReturnSeeds.Field.RegistrationNo, oData.RegNo, ms_oLog);

				oSeeds.Set(VatReturnSeeds.Field.BusinessName, oData.BusinessName, ms_oLog);
				oSeeds.Set(VatReturnSeeds.Field.BusinessAddress, oData.BusinessAddress.Split('\n'), ms_oLog);

				if (!oSeeds.AreBusinessDetailsValid()) {
					sStateError = "Invalid business details detected: " + oSeeds.FatalError;
					return null;
				} // if

				foreach (KeyValuePair<int, decimal> pair in oPeriod.BoxData) {
					int nBoxNum = pair.Key;
					decimal nAmount = pair.Value;

					string sFieldName = oData.BoxNames.ContainsKey(nBoxNum) ? oData.BoxNames[nBoxNum] : " (Box " + nBoxNum + ")";

					oSeeds.ReturnDetails[sFieldName] = new Coin(nAmount, "GBP");

					ms_oLog.Debug("VatReturnSeeds.ReturnDetails[{0}] = {1}", sFieldName, nAmount);
				} // for each box

				oResult.Add(smd, oSeeds);
			} // for each period

			return oResult;
		} // ThrashManualData

		#endregion method ThrashManualData

		#region method DoSave

		private JsonResult DoSave(Hopper oHopper, string sNoAccountError, Customer oCustomer) {
			IDatabaseCustomerMarketPlace mp = FindOrCreateMarketplace(oCustomer);

			if (mp == null) {
				ms_oLog.Alert("Marketplace neither found nor created.");
				return CreateJsonError(sNoAccountError);
			} // if

			Connector.SetBackdoorData(m_oVendorInfo.Name, mp.Id, oHopper);

			try {
				m_oServiceClient.Instance.MarketplaceInstantUpdate(mp.Id);
				mp.Marketplace.GetRetrieveDataHelper(m_oDatabaseHelper).CustomerMarketplaceUpdateAction(mp.Id);
			}
			catch (Exception e) {
				return CreateJsonError("Account has been linked but error occurred while storing the data: " + e.Message);
			} // try

			return CreateJsonNoError();
		} // DoSave

		#endregion method DoSave

		#region method FindMarketplace

		private IDatabaseCustomerMarketPlace FindMarketplace(Customer oCustomer) {
			MP_CustomerMarketPlace oMp = oCustomer.CustomerMarketPlaces.FirstOrDefault(mp => {
				if (mp.Marketplace.InternalId != m_oVendorInfo.Guid())
					return false;

				if (mp.DisplayName != oCustomer.Name)
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

		private IDatabaseCustomerMarketPlace FindOrCreateMarketplace(Customer oCustomer) {
			IDatabaseCustomerMarketPlace oMp = FindMarketplace(oCustomer);

			if (oMp != null)
				return oMp;

			lock (ms_oLockCreateMarketplace) {
				oMp = FindMarketplace(oCustomer);

				if (oMp != null)
					return oMp;

				var model = new AccountModel {
					accountTypeName = m_oVendorInfo.Name,
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

		#endregion method FindOrCreateMarketplace

		#region class AddAccountState

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

		#endregion class AddAccountState

		#region method ValidateMpUniqueness

		private AddAccountState ValidateMpUniqueness(AccountModel model, Customer oCustomer) {
			var oResult = new AddAccountState();

			try {
				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				m_oUniquenessChecker.Check(oResult.Marketplace.InternalId, oCustomer, model.Fill().UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException) {
				oResult.Error = new HmrcManualAccountManagerException(DbStrings.StoreAddedByYou);
			}
			catch (MarketPlaceIsAlreadyAddedException) {
				oResult.Error = new HmrcManualAccountManagerException(DbStrings.StoreAlreadyExistsInDb);
			}
			catch (Exception e) {
				ms_oLog.Error(e);
				oResult.Error = new HmrcManualAccountManagerException(e);
			} // try

			return oResult;
		} // ValidateMpUniqueness

		#endregion method ValidateMpUniqueness

		#region method SaveMarketplace

		private void SaveMarketplace(AddAccountState oState, AccountModel model, Customer oCustomer) {
			try {
				model.id = m_oMpTypes.GetAll().First(a => a.InternalId == m_oVendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = m_oDatabaseHelper.SaveOrUpdateEncryptedCustomerMarketplace(
					model.name,
					oState.Marketplace,
					model,
					oCustomer
				);
				m_oSession.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				ms_oLog.Error(e);
				oState.Error = new HmrcManualAccountManagerException(e);
			} // try

			if (oState.Error != null)
				return;

			try {
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				m_oServiceClient.Instance.UpdateMarketplace(oCustomer.Id, oState.CustomerMarketPlace.Id, true);
			}
			catch (Exception e) {
				ms_oLog.Warn(e,
					"Failed to start UpdateMarketplace strategy for customer [{0}: {1}] with marketplace id {2}," +
					" if this is the only customer marketplace underwriter should run this strategy manually" +
					" (otherwise Main strategy will be stuck).",
					oCustomer.Id,
					oCustomer.Name,
					oState.CustomerMarketPlace.Id
				);
			} // try
		} // SaveMarketplace

		#endregion method SaveMarketplace

		#region fields

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(HmrcManualAccountManager));

		private readonly MarketPlaceRepository m_oMpTypes;
		private readonly CGMPUniqChecker m_oUniquenessChecker;
		private readonly DatabaseDataHelper m_oDatabaseHelper;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession m_oSession;
		private readonly VendorInfo m_oVendorInfo;
		private readonly CustomerRepository m_oCustomers;

		private static volatile VendorInfo ms_oVendorInfo;
		private static readonly object ms_oLockVendorInfo = new object();

		private static readonly object ms_oLockCreateMarketplace = new object();

		#endregion fields

		#endregion private
	} // class HmrcManualAccountManager
} // namespace
