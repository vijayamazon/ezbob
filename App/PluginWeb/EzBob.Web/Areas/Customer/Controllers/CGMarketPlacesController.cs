using System;
using System.Collections.Generic;
using System.IO;
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
				ViewData["error"] = JsonConvert.SerializeObject(oState.Error);
				ViewData["model"] = "";
				return View();
			} // if

			List<VatReturnSeeds> oSeeds = ValidateFiles(Request.Files, oState);

			if (oState.Error != null) {
				ViewData["error"] = JsonConvert.SerializeObject(oState.Error);
				ViewData["model"] = "";
				return View();
			} // if

			if (oSeeds.Count < 1) {
				ViewData["error"] = JsonConvert.SerializeObject(new { error = "No files accepted." });
				ViewData["model"] = "";
				return View();
			} // if

			SaveMarketplace(oState, model);

			if (oState.Error != null) {
				ViewData["error"] = JsonConvert.SerializeObject(oState.Error);
				ViewData["model"] = "";
				return View();
			} // if

			// TODO: save file data to DB (as if connector fetched it from remote source)

			ViewData["error"] = "";
			ViewData["model"] = JsonConvert.SerializeObject(oState.Model);
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

			public AddAccountState() {
				VendorInfo = null;
				AccountData = null;
				Marketplace = null;
				Error = null;
				Model = null;
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
				oResult.Error = this.JsonNet(new { error = sError });
				return oResult;
			} // try

			try {
				oResult.AccountData = model.Fill();

				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);

				_mpChecker.Check(oResult.Marketplace.InternalId, _context.Customer, oResult.AccountData.UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException ) {
				oResult.Error = this.JsonNet(new { error = DbStrings.StoreAddedByYou });
				return oResult;
			}
			catch (MarketPlaceIsAlreadyAddedException ) {
				oResult.Error = this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
				return oResult;
			}
			catch (Exception e) {
				Log.Error(e);
				oResult.Error = this.JsonNet(new { error = e.Message });
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
					oState.Error = this.JsonNet(new { error = cge.Message });
				} // if

				Log.ErrorFormat("Failed to validate {0} account, continuing with registration.", model.accountTypeName);
				Log.Error(cge);

				// Error is logged but not written into state.
			}
			catch (ApiException cge) {
				Log.ErrorFormat("Failed to validate {0} account.", model.accountTypeName);
				Log.Error(cge);
				oState.Error = this.JsonNet(new { error = cge.Message });
			}
			catch (InvalidCredentialsException ice) {
				Log.Info(ice);
				oState.Error = this.JsonNet(new { error = ice.Message });
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = this.JsonNet(new { error = e.Message });
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
			}
			catch (Exception e) {
				Log.Error(e);
				oState.Error = this.JsonNet(new { error = e.Message });
			} // try
		} // SaveMarketplace

		#endregion method SaveMarketplace

		#region method ValidateFiles

		private List<VatReturnSeeds> ValidateFiles(HttpFileCollectionBase oFiles, AddAccountState oState) {
			var oOutput = new List<VatReturnSeeds>();

			for (int i = 0; i < oFiles.Count; i++) {
				HttpPostedFileBase oFile = oFiles[i];

				if (oFile == null)
					continue;

				var oBuffer = new MemoryStream();

				var buf = new byte[32768];

				int nRead = oFile.InputStream.Read(buf, 0, buf.Count());

				while (nRead > 0) {
					oBuffer.Write(buf, 0, nRead);
					nRead = oFile.InputStream.Read(buf, 0, buf.Count());
				} // while

				var vrpt = new VatReturnPdfThrasher(false, new SafeILog(Log));

				var smd = new SheafMetaData {
					BaseFileName = oFile.FileName,
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = null
				};

				ISeeds oResult = null;

				try {
					oResult = vrpt.Run(smd, oBuffer.ToArray());
				}
				catch (Exception e) {
					Log.WarnFormat("Failed to parse uploaded file {0}", oFile.FileName);
					Log.Warn(e);
					continue;
				} // try

				if (oResult != null)
					oOutput.Add((VatReturnSeeds)oResult);
			} // for

			if (oOutput.Count < 1)
				return oOutput;

			// TODO: validate registration number and dates
			// return new List<> if something goes wrong
			// it is possible to use oState.Error if needed
			
			return oOutput;
		} // ValidateFiles

		#endregion method ValidateFiles

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