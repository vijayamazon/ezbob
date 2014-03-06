namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using Code;
	using CompanyFiles;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceReference;
	using Infrastructure;
	using Scorto.Web;
	using log4net;
	using NHibernate;
	using System.Data;
	using EZBob.DatabaseLib;

	public class CompanyFilesMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly Customer _customer;
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;
		private readonly EzServiceClient m_oServiceClient;

		public CompanyFilesMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			ISession session)
		{
			_context = context;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_session = session;
			_helper = helper;
			m_oServiceClient = ServiceClient.Instance;
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult Accounts()
		{
			var oEsi = new CompanyFilesServiceInfo();

			var companyFiles = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(x => x.DisplayName)
				.ToList();
			return this.JsonNet(companyFiles);
		}

		[HttpPost]
		public System.Web.Mvc.ActionResult UploadedFiles()
		{
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			for (int i = 0; i < Request.Files.Count; ++i)
			{
				HttpPostedFileBase file = Request.Files[i];
				if (file != null)
				{
					var content = new byte[file.ContentLength];

					int nRead = file.InputStream.Read(content, 0, file.ContentLength);

					if (nRead != file.ContentLength)
					{
						Log.WarnFormat("File {0}: failed to read entire file contents, ignoring.", i);
						continue;
					} // if
					ServiceClient.Instance.CompanyFilesUpload(_context.Customer.Id, file.FileName, content);
				}
			}
			return Json(new { });
		} // UploadedFiles

		[HttpPost]
		public System.Web.Mvc.ActionResult Connect()
		{
			try
			{
				var serviceInfo = new CompanyFilesServiceInfo();
				var name = serviceInfo.DisplayName;
				var cf = new CompanyFilesDatabaseMarketPlace();
				var mp = _helper.SaveOrUpdateCustomerMarketplace(name, cf, null, _context.Customer);

				_session.Flush();
				m_oServiceClient.UpdateMarketplace(_context.Customer.Id, mp.Id, true);

			}
			catch (Exception e)
			{
				Log.Error(e);
			} // try

			return Json(new { });
		}



		//[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		//[Ajax]
		//[HttpPost]
		//public JsonNetResult Accounts(EkmAccountModel model)
		//{
		//	string errorMsg;
		//	if (!_validator.Validate(model.login, model.password, out errorMsg))
		//	{
		//		var errorObject = new { error = errorMsg };
		//		return this.JsonNet(errorObject);
		//	}
		//	try
		//	{
		//		var customer = _context.Customer;
		//		var username = model.login;
		//		var ekm = new EkmDatabaseMarketPlace();
		//		_mpChecker.Check(ekm.InternalId, customer, username);
		//		var oEsi = new EkmServiceInfo();
		//		int marketPlaceId = _mpTypes
		//			.GetAll()
		//			.First(a => a.InternalId == oEsi.InternalId)
		//			.Id;

		//		var ekmSecurityInfo = new EkmSecurityInfo { MarketplaceId = marketPlaceId, Name = username, Password = model.password };

		//		var mp = _helper.SaveOrUpdateCustomerMarketplace(username, ekm, ekmSecurityInfo.Password, customer);

		//		_session.Flush();

		//		_appCreator.CustomerMarketPlaceAdded(customer, mp.Id);

		//		return this.JsonNet(EkmAccountModel.ToModel(mp));
		//	}
		//	catch (MarketPlaceAddedByThisCustomerException e)
		//	{
		//		Log.Debug(e);
		//		return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
		//	}
		//	catch (MarketPlaceIsAlreadyAddedException e)
		//	{
		//		Log.Debug(e);
		//		return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
		//	}
		//	catch (Exception e)
		//	{
		//		Log.Error(e);
		//		return this.JsonNet(new { error = e.Message });
		//	}
		//}

		private JsonResult CreateError(string sErrorMsg)
		{
			return Json(new { error = sErrorMsg });
		} // CreateError

	}
}