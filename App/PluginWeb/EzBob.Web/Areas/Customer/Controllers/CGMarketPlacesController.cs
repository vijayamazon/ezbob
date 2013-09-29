using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Configuration;
using Ezbob.HmrcHarvester;
using EzBob.Web.Infrastructure;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using EzBob.Web.ApplicationCreator;
using Ezbob.Logger;
using Integration.ChannelGrabberConfig;
using Integration.ChannelGrabberFrontend;
using Newtonsoft.Json;
using log4net;
using Scorto.Web;

namespace EzBob.Web.Areas.Customer.Controllers {
	using NHibernate;

	public class CGMarketPlacesController : Controller {
		#region public

		#region constructor

		public CGMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session,
			IAppCreator appCreator) {
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
			_session = session;
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

			AccountModel model = JsonConvert.DeserializeObject<AccountModel>(Request["model"]);

			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null) {
				ViewError = oState.Error;
				ViewModel = null;
				return View();
			} // if

			Hopper oSeeds = ValidateFiles(Request.Files, oState);

			if (oState.Error != null) {
				ViewError = oState.Error;
				ViewModel = null;
				return View();
			} // if

			if (oSeeds == null) {
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

			Integration.ChannelGrabberFrontend.Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try {
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e) {
				ViewError = CreateError("Account has been linked but error occured while storing uploaded data: " + e.Message);
			} // try

			return View();
		} // HandleUploadedHmrcVatReturn

		#endregion method HandleUploadedHmrcVatReturn

		#region method Accounts (account list by type)

		[Transactional]
		public JsonNetResult Accounts(string atn) {
			var oVsi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(atn);

			return this.JsonNet(_context.Customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oVsi.Guid())
				.Select(AccountModel.ToModel)
				.ToList()
			);
		} // Accounts

		#endregion method Accounts (account list by type)

		#region method Accounts (add new account)

		[Transactional]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(AccountModel model) {
			AddAccountState oState = ValidateModel(model);

			if (oState.Error != null)
				return oState.Error;

			ValidateAccount(oState, model);

			if (oState.Error != null)
				return oState.Error;

			SaveMarketplace(oState, model);

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
			} // constructor
		} // class AddAccountState

		#endregion class AddAccountState

		#region method ValidateModel

		private AddAccountState ValidateModel(AccountModel model) {
			var oResult = new AddAccountState();

			oResult.VendorInfo = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(model.accountTypeName);

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

		private void SaveMarketplace(AddAccountState oState, AccountModel model) {
			try {
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				if (_context.Customer.WizardStep != WizardStepType.AllStep)
					_context.Customer.WizardStep = WizardStepType.Marketplace;

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

		#endregion method SaveMarketplace

		#region method ValidateFiles

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
				oDateIntervals.Sort(DateInterval.CompareForSort);

				DateInterval next = null;

				foreach (DateInterval cur in oDateIntervals) {
					if (next == null) {
						next = cur;
						continue;
					} // if

					DateInterval prev = next;
					next = cur;

					if (!prev.Follows(next)) {
						oState.Error = CreateError("Inconsequent date ranges: " + prev + " and " + next);
						return null;
					} // if
				} // for each interval

				return oOutput;
			} // switch
		} // ValidateFiles

		#endregion method ValidateFiles

		#region class DateInterval

		private class DateInterval : Tuple<DateTime, DateTime> {
			#region public

			#region method CompareForSort

			public static int CompareForSort(DateInterval a, DateInterval b) { return a.Start.CompareTo(b.Start); } // CompareForSort

			#endregion method CompareForSort

			#region constructor

			public DateInterval(DateTime oStart, DateTime oEnd) : base(Min(oStart, oEnd), Max(oStart, oEnd)) {} // constructor

			#endregion constructor

			#region method Intersects

			public bool Intersects(DateInterval di) {
				if ((Start <= di.Start) && (di.End <= End))
					return true;

				if ((di.Start <= Start) && (End <= di.End))
					return true;

				if ((Start <= di.Start) && (di.Start <= End))
					return true;

				if ((Start <= di.End) && (di.End <= End))
					return true;

				return false;
			} // Intersects

			#endregion method Intersects

			#region method Follows

			public bool Follows(DateInterval di) {
				return End.Date.AddDays(1) == di.Start.Date;
			} // Follows

			#endregion method Follows

			#region method ToString

			public override string ToString() {
				return string.Format(
					"{0} - {1}",
					Start.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
					End.ToString("MMM d yyyy", CultureInfo.InvariantCulture)
				);
			} // ToString

			#endregion method ToString

			#endregion public

			#region private

			private DateTime Start { get { return Item1; } } // Start
			private DateTime End { get { return Item2; } } // End

			private static DateTime Min(DateTime a, DateTime b) { return (a.Date <= b.Date) ? a.Date : b.Date; } // Min
			private static DateTime Max(DateTime a, DateTime b) { return (a.Date >= b.Date) ? a.Date : b.Date; } // Max

			#endregion private
		} // class DateInterval

		#endregion class DateInterval

		#region method CreateError

		private JsonNetResult CreateError(Exception ex) {
			return CreateError(ex.Message);
		} // CreateError

		private JsonNetResult CreateError(string sErrorMsg) {
			return this.JsonNet(new { error = sErrorMsg });
		} // CreateError

		#endregion method CreateError

		#region property ViewError

		private JsonNetResult ViewError { set {
			ViewData["error"] = JsonConvert.SerializeObject(value);
		} } // ViewError

		#endregion property ViewError

		#region property ViewModel

		private string ViewModel { set { ViewData["model"] = value ?? "null"; } } // ViewModel

		#endregion property ViewModel

		#region fields

		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly DatabaseDataHelper _helper;
		private readonly ISession _session;

		private static readonly ILog Log = LogManager.GetLogger(typeof(CGMarketPlacesController));

		#endregion fields

		#endregion private
	} // class CGMarketPlacesController
} // namespace