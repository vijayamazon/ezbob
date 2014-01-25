namespace EzBob.Web.Areas.Customer.Controllers 
{
	using Code.ApplicationCreator;
	using NHibernate;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.HmrcHarvester;
	using Infrastructure;
	using Code.MpUniq;
	using Web.Models.Strings;
	using Ezbob.Logger;
	using Ezbob.ValueIntervals;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using Newtonsoft.Json;
	using log4net;
	using Scorto.Web;

	public class HmrcController : Controller 
	{
		public HmrcController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			ISession session,
			IAppCreator appCreator) {
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_appCreator = appCreator;
			_session = session;
		}


		[HttpPost]
		public ActionResult UploadFiles()
		{
			return null;
		}

		[HttpPost]
		public ActionResult UploadedFiles()
		{
			Response.AddHeader("x-frame-options", "SAMEORIGIN");
			AccountModel model = new AccountModel {accountTypeName = "HMRC", displayName = string.Empty, name = string.Empty};

			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null)
			{
				ViewError = oState.Error;
				ViewModel = null;
				return null;
			} // if

			Hopper oSeeds = ValidateFiles(Request.Files, oState);

			if (oState.Error != null)
			{
				ViewError = oState.Error;
				ViewModel = null;
				return null;
			} // if

			if (oSeeds == null)
			{
				ViewError = CreateError("No files accepted.");
				ViewModel = null;
				return null;
			} // if

			// TODO: The following code should happen after upload is clicked
			SaveMarketplace(oState, model);

			if (oState.Error != null)
			{
				ViewError = oState.Error;
				ViewModel = null;
				return null;
			} // if

			ViewModel = JsonConvert.SerializeObject(oState.Model);
			ViewError = null;

			Connector.SetRunningInWebEnvFlag(model.accountTypeName, oState.CustomerMarketPlace.Id);
			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try
			{
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e)
			{
				ViewError = CreateError("Account has been linked but error occured while storing uploaded data: " + e.Message);
			} // try

			return null;
		}

		private class AddAccountState {
			public VendorInfo VendorInfo;
			public AccountData AccountData;
			public IMarketplaceType Marketplace;
			public JsonNetResult Error;
			public JsonNetResult Model;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState() {
				VendorInfo = null;
				AccountData = null;
				Marketplace = null;
				Error = null;
				Model = null;
				CustomerMarketPlace = null;
			}
		}

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
		
		private void SaveMarketplace(AddAccountState oState, AccountModel model) {
			try {
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(model.name, oState.Marketplace, model, _context.Customer);
				_session.Flush();
				_appCreator.CustomerMarketPlaceAdded(_context.Customer, mp.Id);

				oState.Model = this.JsonNet(AccountModel.ToModel(mp));
				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e);
			} // try
		} // SaveMarketplace


		private Hopper ValidateFiles(HttpFileCollectionBase oFiles, AddAccountState oState) {
			var oOutput = new Hopper();

			long? nRegistrationNo = null;
			var oDateIntervals = new List<DateInterval>();
			int nAddedCount = 0;

			for (int i = 0; i < oFiles.Count; i++) {
				HttpPostedFileBase oFile = oFiles[i];

				if (oFile == null) {
					Log.DebugFormat("File {0}: not found, ignoring.", i);
					continue;
				} // if

				Log.DebugFormat("File {0}, name: {1}", i, oFile.FileName);

				if (oFile.ContentLength == 0) {
					Log.DebugFormat("File {0}: is empty, ignoring.", i);
					continue;
				} // if

				if (oFile.ContentType.Trim().ToLower() != "application/pdf") {
					Log.DebugFormat("File {0}: is not PDF content type, ignoring.", i);
					continue;
				} // if

				var oFileContents = new byte[oFile.ContentLength];

				int nRead = oFile.InputStream.Read(oFileContents, 0, oFile.ContentLength);

				if (nRead != oFile.ContentLength) {
					Log.WarnFormat("File {0}: failed to read entire file contents, ignoring.", i);
					continue;
				} // if

				var smd = new SheafMetaData {
					BaseFileName = oFile.FileName,
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null
				};

				oOutput.Add(smd, oFileContents);

				var vrpt = new VatReturnPdfThrasher(false, new SafeILog(Log));
				ISeeds oResult = null;

				try {
					oResult = vrpt.Run(smd, oFileContents);
				}
				catch (Exception e) {
					Log.WarnFormat("Failed to parse file {0} named {1}:", i, oFile.FileName);
					Log.Warn(e);
					continue;
				} // try

				if (oResult == null)
					continue;

				var oSeeds = (VatReturnSeeds)oResult;

				if (nRegistrationNo.HasValue) {
					if (nRegistrationNo.Value != oSeeds.RegistrationNo) {
						oState.Error = CreateError("Inconsistent business registration number.");
						return null;
					} // if
				}
				else
					nRegistrationNo = oSeeds.RegistrationNo;

				var di = new DateInterval(oSeeds.DateFrom, oSeeds.DateTo);

				foreach (DateInterval oInterval in oDateIntervals) {
					if (oInterval.Intersects(di)) {
						oState.Error = CreateError("Inconsistent date ranges: " + oInterval + " and " + di);
						return null;
					} // if
				} // for each

				oDateIntervals.Add(di);
				oOutput.Add(smd, oResult);
				nAddedCount++;
			} // for

			switch (nAddedCount) {
			case 0:
				return null;

			case 1:
				return oOutput;

			default:
				oDateIntervals.Sort((a, b) => a.Left.CompareTo(b.Left));

				DateInterval next = null;

				foreach (DateInterval cur in oDateIntervals) {
					if (next == null) {
						next = cur;
						continue;
					} // if

					DateInterval prev = next;
					next = cur;

					if (!prev.IsJustBefore(next)) {
						oState.Error = CreateError("Inconsequent date ranges: " + prev + " and " + next);
						return null;
					} // if
				} // for each interval

				return oOutput;
			}
		}

		private JsonNetResult CreateError(Exception ex) {
			return CreateError(ex.Message);
		}

		private JsonNetResult CreateError(string sErrorMsg) {
			return this.JsonNet(new { error = sErrorMsg });
		}

		private JsonNetResult ViewError { set {
			ViewData["error"] = JsonConvert.SerializeObject(value);
		} } 

		private string ViewModel { set { ViewData["model"] = value ?? "null"; } } // ViewModel


		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly IAppCreator _appCreator;
		private readonly DatabaseDataHelper _helper;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(HmrcController));
	}
}