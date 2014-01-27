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
			var oState = Session["oState"] as AddAccountState;
			var model = Session["model"] as AccountModel;
			var oSeeds = Session["oSeeds"] as Hopper;

			if (oState == null || model == null || oSeeds == null)
			{
				return this.JsonNet(new { error = "Failure during files upload"});
			}

			SaveMarketplace(oState, model);

			if (oState.Error != null)
			{
				return oState.Error;
			} // if

			Connector.SetRunningInWebEnvFlag(model.accountTypeName, oState.CustomerMarketPlace.Id);
			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try
			{
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e)
			{
				return CreateError("Account has been linked but error occured while storing uploaded data: " + e.Message);
			} // try

			return this.JsonNet(new {});
		}

		[HttpPost]
		public ActionResult UploadedFiles()
		{
			Response.AddHeader("x-frame-options", "SAMEORIGIN");
			string customerEmail = _context.Customer.Name;
			var model = new AccountModel { accountTypeName = "HMRC", displayName = customerEmail, name = customerEmail, login = customerEmail, password = "topsecret" };

			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null)
			{
				return oState.Error;
			} // if

			Hopper oSeeds = ValidateFiles(Request.Files, oState);

			if (oState.Error != null)
			{
				return oState.Error;
			} // if

			if (oSeeds == null)
			{
				return CreateError("No files accepted.");
			} // if

			Session["oState"] = oState;
			Session["model"] = model;
			Session["oSeeds"] = oSeeds;

			return this.JsonNet(new { });
		}

		private class AddAccountState {
			public VendorInfo VendorInfo;
			public IMarketplaceType Marketplace;
			public JsonNetResult Error;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState() {
				VendorInfo = null;
				Marketplace = null;
				Error = null;
				CustomerMarketPlace = null;
			}
		}

		private AddAccountState ValidateModel(AccountModel model) {
			var oResult = new AddAccountState {VendorInfo = Configuration.Instance.GetVendorInfo(model.accountTypeName)};

			if (oResult.VendorInfo == null) {
				var sError = "Unsupported account type: " + model.accountTypeName;
				Log.Error(sError);
				oResult.Error = CreateError(sError);
				return oResult;
			} // try

			try {
				model.Fill();
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
				ISeeds oResult;

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
		
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly IAppCreator _appCreator;
		private readonly DatabaseDataHelper _helper;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(HmrcController));
	}
}