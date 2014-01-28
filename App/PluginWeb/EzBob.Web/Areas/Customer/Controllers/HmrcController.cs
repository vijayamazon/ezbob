namespace EzBob.Web.Areas.Customer.Controllers 
{
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
			CGMPUniqChecker mpChecker,
			ISession session) {
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_mpChecker = mpChecker;
			_session = session;
		}
		
		[HttpPost]
		public ActionResult UploadFiles()
		{
			var oState = Session["oState"] as AddAccountState;
			var model = Session["model"] as AccountModel;

			if (oState == null || model == null)
			{
				return this.JsonNet(new { error = "Failure during files upload"});
			}

			string stateError;
			Hopper oSeeds = GetProcessedFiles(out stateError);

			if (stateError != null)
			{
				return CreateError(stateError);
			} // if

			SaveMarketplace(oState, model);

			if (oState.Error != null)
			{
				return oState.Error;
			} // if

			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try
			{
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
				Customer customer = _context.Customer;
				if (!customer.WizardStep.TheLastOne)
				{
					customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.Marketplace);
					Log.DebugFormat("Customer {1} ({0}): wizard step has been updated to :{2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.Marketplace);
				}
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

			string stateError = ProcessAndStoreFiles(Request.Files);
			if (stateError != null)
			{
				return CreateError(stateError);
			}

			if (Session["oState"] == null)
			{
				Session["oState"] = oState;
				Session["model"] = model;
			}

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
				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				_mpChecker.Check(oResult.Marketplace.InternalId, _context.Customer, model.Fill().UniqueID());
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

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = CreateError(e);
			} // try
		} // SaveMarketplace

		public static Hopper ValidateFiles(HttpFileCollectionBase oFiles, out string stateError)
		{
			stateError = null;
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
						stateError = "Inconsistent business registration number.";
						return null;
					} // if
				}
				else
					nRegistrationNo = oSeeds.RegistrationNo;

				var di = new DateInterval(oSeeds.DateFrom, oSeeds.DateTo);

				foreach (DateInterval oInterval in oDateIntervals) {
					if (oInterval.Intersects(di)) {
						stateError = "Inconsistent date ranges: " + oInterval + " and " + di;
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
						stateError = "Inconsequent date ranges: " + prev + " and " + next;
						return null;
					} // if
				} // for each interval

				return oOutput;
			}
		}

		public string ProcessAndStoreFiles(HttpFileCollectionBase oFiles)
		{
			if (Session["Hopper"] == null)
			{
				Session["Hopper"] = new Hopper();
			}
			if (Session["AddedCount"] == null)
			{
				Session["AddedCount"] = 0;
			}
			if (Session["DateIntervals"] == null)
			{
				Session["DateIntervals"] = new List<DateInterval>();
			}

			long? nRegistrationNo = null;

			for (int i = 0; i < oFiles.Count; i++)
			{
				HttpPostedFileBase oFile = oFiles[i];

				if (oFile == null)
				{
					Log.DebugFormat("File {0}: not found, ignoring.", i);
					continue;
				} // if

				Log.DebugFormat("File {0}, name: {1}", i, oFile.FileName);

				if (oFile.ContentLength == 0)
				{
					Log.DebugFormat("File {0}: is empty, ignoring.", i);
					continue;
				} // if

				if (oFile.ContentType.Trim().ToLower() != "application/pdf")
				{
					Log.DebugFormat("File {0}: is not PDF content type, ignoring.", i);
					continue;
				} // if

				var oFileContents = new byte[oFile.ContentLength];

				int nRead = oFile.InputStream.Read(oFileContents, 0, oFile.ContentLength);

				if (nRead != oFile.ContentLength)
				{
					Log.WarnFormat("File {0}: failed to read entire file contents, ignoring.", i);
					continue;
				} // if

				var smd = new SheafMetaData
				{
					BaseFileName = oFile.FileName,
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null
				};

				(Session["Hopper"] as Hopper).Add(smd, oFileContents);

				var vrpt = new VatReturnPdfThrasher(false, new SafeILog(Log));
				ISeeds oResult;

				try
				{
					oResult = vrpt.Run(smd, oFileContents);
				}
				catch (Exception e)
				{
					Log.WarnFormat("Failed to parse file {0} named {1}:", i, oFile.FileName);
					Log.Warn(e);
					continue;
				} // try

				if (oResult == null)
					continue;

				var oSeeds = (VatReturnSeeds)oResult;

				if (nRegistrationNo.HasValue)
				{
					if (nRegistrationNo.Value != oSeeds.RegistrationNo)
					{
						return "Inconsistent business registration number.";
					} // if
				}
				else
					nRegistrationNo = oSeeds.RegistrationNo;

				var di = new DateInterval(oSeeds.DateFrom, oSeeds.DateTo);

				foreach (DateInterval oInterval in (Session["DateIntervals"] as List<DateInterval>))
				{
					if (oInterval.Intersects(di))
					{
						return "Inconsistent date ranges: " + oInterval + " and " + di;
					} // if
				} // for each

				(Session["DateIntervals"] as List<DateInterval>).Add(di);
				(Session["Hopper"] as Hopper).Add(smd, oResult);
				Session["AddedCount"] = (int)Session["AddedCount"] + 1;
			} // for
			return null;
		}

		public Hopper GetProcessedFiles(out string stateError)
		{
			stateError = null;
			switch ((int)Session["AddedCount"])
			{
				case 0:
					stateError = "No files were successfully processed";
					return null;

				case 1:
					return (Session["Hopper"] as Hopper);

				default:
					(Session["DateIntervals"] as List<DateInterval>).Sort((a, b) => a.Left.CompareTo(b.Left));

					DateInterval next = null;

					foreach (DateInterval cur in (Session["DateIntervals"] as List<DateInterval>))
					{
						if (next == null)
						{
							next = cur;
							continue;
						} // if

						DateInterval prev = next;
						next = cur;

						if (!prev.IsJustBefore(next))
						{
							stateError = "Inconsequent date ranges: " + prev + " and " + next;
							return null;
						} // if
					} // for each interval

					return (Session["Hopper"] as Hopper);
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
		private readonly CGMPUniqChecker _mpChecker;
		private readonly DatabaseDataHelper _helper;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(HmrcController));
	}
}