namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using Code;
	using CompanyFiles;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Scorto.Web;
	using log4net;
	using NHibernate;
	using System.Data;
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
		public ActionResult UploadedFiles()
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
				var serviceInfo = new CompanyFilesServiceInfo();
				var name = serviceInfo.DisplayName;
				var cf = new CompanyFilesDatabaseMarketPlace();
				var mp = _helper.SaveOrUpdateCustomerMarketplace(_context.Customer.Name + "_" + name, cf, null, _context.Customer);

				_session.Flush();
				m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, mp.Id, true);

				mp.Marketplace.GetRetrieveDataHelper(_helper).UpdateCustomerMarketplaceFirst(mp.Id);
			}
			catch (Exception e)
			{
				Log.Error(e);
			} // try

			return Json(new { });
		}
	}
}