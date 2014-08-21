namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using CompanyFiles;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Infrastructure.Attributes;
	using ServiceClientProxy;
	using log4net;
	using NHibernate;
	using EZBob.DatabaseLib;

	public class CompanyFilesMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CompanyFilesMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly Customer _customer;
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;

		public CompanyFilesMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			ISession session)
		{
			_context = context;
			_customer = context.Customer;
			_session = session;
			_helper = helper;
			m_oServiceClient = new ServiceClient();
		}

		public JsonResult Accounts()
		{
			var oEsi = new CompanyFilesServiceInfo();

			var companyFiles = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(x => x.DisplayName)
				.ToList();

			return Json(companyFiles, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult UploadedFiles()
		{
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			OneUploadLimitation oLimitations = CurrentValues.Instance.GetUploadLimitations("CompanyFilesMarketPlaces", "UploadedFiles");

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

					string sMimeType = oLimitations.DetectFileMimeType(content, file.FileName);

					if (string.IsNullOrWhiteSpace(sMimeType)) {
						Log.WarnFormat("Not saving file #" + (i + 1) + ": " + file.FileName + " because it has unsupported MIME type.");
						continue;
					} // if

					m_oServiceClient.Instance.CompanyFilesUpload(_context.Customer.Id, file.FileName, content, file.ContentType);
				}
			}
			return Json(new { });
		} // UploadedFiles

		[HttpPost]
		public ActionResult Connect()
		{
			try
			{
				var mpId = ConnectTrn();
				if (mpId != -1)
				{
					m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, mpId, true, _context.UserId);
					m_oServiceClient.Instance.MarketplaceInstantUpdate(mpId);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			} // try

			return Json(new { });
		}

		private int ConnectTrn() {
			int nResult = -1;

			try {
				new Transactional(() => {
					var serviceInfo = new CompanyFilesServiceInfo();
					var name = serviceInfo.DisplayName;
					var cf = new CompanyFilesDatabaseMarketPlace();
					var mp = _helper.SaveOrUpdateCustomerMarketplace(_context.Customer.Name + "_" + name, cf, null, _context.Customer);
					var rdh = mp.Marketplace.GetRetrieveDataHelper(_helper);
					rdh.UpdateCustomerMarketplaceFirst(mp.Id);
					nResult = mp.Id;
				}).Execute();
			}
			catch (Exception ex) {
				Log.Error(ex);
			}

			return nResult;
		}
	}
}