namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using Code.ApplicationCreator;
	using Code.MpUniq;
	using Customer.Controllers;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.HmrcHarvester;
	using Ezbob.ValueIntervals;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using Web.Models.Strings;
	using log4net;

	public class UploadHmrcController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(UploadHmrcController));
		private readonly CustomerRepository _customers;
		private readonly IAppCreator _appCreator;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly ISession _session;
		private readonly CGMPUniqChecker _mpChecker;
		private readonly DatabaseDataHelper _helper;

		public UploadHmrcController(CustomerRepository customers,
			IAppCreator appCreator,
			CGMPUniqChecker mpChecker,
			IRepository<MP_MarketplaceType> mpTypes, 
			DatabaseDataHelper helper,
			ISession session)
		{
			_appCreator = appCreator;
			_customers = customers;
			_mpChecker = mpChecker;
			_helper = helper;
			_mpTypes = mpTypes;
			_session = session;

		}

		#region HMRC Upload
		[HttpPost]
		public ActionResult UploadHmrc(int customerId)
		{
			if (customerId == 0) return Json(new { error = "Customer not specified" });

			//var customer = _customers.Get(customerId);
			var oProcessor = new SessionHmrcFileProcessor(Session, customerId, Request.Files);

			oProcessor.Run();

			if (!string.IsNullOrWhiteSpace(oProcessor.FileCache.ErrorMsg))
				return Json(new { error = oProcessor.FileCache.ErrorMsg });

			return Json(new { });
		}

		[HttpPost]
		public ActionResult UploadFiles(int customerId)
		{
			HmrcFileCache oFileCache = HmrcFileCache.Get(Session);

			if ((oFileCache != null) && !string.IsNullOrWhiteSpace(oFileCache.ErrorMsg))
				return CreateError(oFileCache.ErrorMsg);

			var customer = _customers.Get(customerId);
			string customerEmail = customer.Name;
			var model = new AccountModel { accountTypeName = "HMRC", displayName = customerEmail, name = customerEmail, login = customerEmail, password = "topsecret" };

			AddAccountState oState = ValidateHmrcModel(model, customer);

			if (oState.Error != null)
				return oState.Error;

			string stateError;
			Hopper oSeeds = GetProcessedFiles(out stateError);

			if (stateError != null)
				return CreateError(stateError);

			SaveMarketplace(oState, model, customer);

			if (oState.Error != null)
				return oState.Error;

			Connector.SetBackdoorData(model.accountTypeName, oState.CustomerMarketPlace.Id, oSeeds);

			try
			{
				// This is done to for two reasons:
				// 1. update Customer.WizardStep to WizardStepType.Marketplace
				// 2. insert entries into EzServiceActionHistory
				_appCreator.CustomerMarketPlaceAdded(customer, oState.CustomerMarketPlace.Id);
			}
			catch (Exception e)
			{
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

			try
			{
				oState.CustomerMarketPlace.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(oState.CustomerMarketPlace.Id);
			}
			catch (Exception e)
			{
				return CreateError("Account has been linked but error occured while storing uploaded data: " + e.Message);
			} // try

			return Json(new { });
		} // UploadFiles

		private AddAccountState ValidateHmrcModel(AccountModel model, Customer customer)
		{
			var oResult = new AddAccountState { VendorInfo = Configuration.Instance.GetVendorInfo(model.accountTypeName) };

			if (oResult.VendorInfo == null)
			{
				var sError = "Unsupported account type: " + model.accountTypeName;
				Log.Error(sError);
				oResult.Error = CreateError(sError);
				return oResult;
			} // try

			try
			{
				oResult.Marketplace = new DatabaseMarketPlace(model.accountTypeName);
				_mpChecker.Check(oResult.Marketplace.InternalId, customer, model.Fill().UniqueID());
			}
			catch (MarketPlaceAddedByThisCustomerException)
			{
				oResult.Error = CreateError(DbStrings.StoreAddedByYou);
				return oResult;
			}
			catch (MarketPlaceIsAlreadyAddedException)
			{
				oResult.Error = CreateError(DbStrings.StoreAlreadyExistsInDb);
				return oResult;
			}
			catch (Exception e)
			{
				Log.Error(e);
				oResult.Error = CreateError(e.Message);
				return oResult;
			} // try

			return oResult;
		} // ValidateModel

		private class AddAccountState
		{
			public VendorInfo VendorInfo;
			public IMarketplaceType Marketplace;
			public JsonResult Error;
			public IDatabaseCustomerMarketPlace CustomerMarketPlace;

			public AddAccountState()
			{
				VendorInfo = null;
				Marketplace = null;
				Error = null;
				CustomerMarketPlace = null;
			} // constructor
		} // class AddAccountState

		private Hopper GetProcessedFiles(out string stateError)
		{
			stateError = null;

			HmrcFileCache oFileCache = HmrcFileCache.Get(Session);

			if (oFileCache == null)
			{
				stateError = "No files were successfully processed";
				return null;
			} // if

			switch (oFileCache.AddedCount)
			{
				case 0:
					stateError = "No files were successfully processed";
					return null;

				case 1:
					return oFileCache.Hopper;

				default:
					oFileCache.DateIntervals.Sort((a, b) => a.Left.CompareTo(b.Left));

					DateInterval next = null;

					foreach (DateInterval cur in oFileCache.DateIntervals)
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

					return oFileCache.Hopper;
			} // switch
		} // GetProcessedFiles

		private void SaveMarketplace(AddAccountState oState, AccountModel model, Customer customer)
		{
			try
			{
				model.id = _mpTypes.GetAll().First(a => a.InternalId == oState.VendorInfo.Guid()).Id;
				model.displayName = model.displayName ?? model.name;

				IDatabaseCustomerMarketPlace mp = _helper.SaveOrUpdateCustomerMarketplace(model.name, oState.Marketplace, model, customer);
				_session.Flush();

				oState.CustomerMarketPlace = mp;
			}
			catch (Exception e)
			{
				Log.Error(e);
				oState.Error = CreateError(e.Message);
			} // try
		} // SaveMarketplace

		private JsonResult CreateError(string sErrorMsg)
		{
			return Json(new { error = sErrorMsg });
		} // CreateError

		#endregion

	}
}
