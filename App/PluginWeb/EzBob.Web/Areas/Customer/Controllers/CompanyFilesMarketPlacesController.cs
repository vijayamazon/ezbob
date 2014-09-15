namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using CompanyFiles;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;
	using Infrastructure;
	using ServiceClientProxy;
	using EZBob.DatabaseLib;

	public class CompanyFilesMarketPlacesController : Controller {
		public CompanyFilesMarketPlacesController(IEzbobWorkplaceContext context, DatabaseDataHelper helper) {
			_context = context;
			_customer = context.Customer;
			_helper = helper;
			m_oServiceClient = new ServiceClient();
		} // constructor

		public JsonResult Accounts() {
			var oEsi = new CompanyFilesServiceInfo();

			var companyFiles = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(x => x.DisplayName)
				.ToList();

			return Json(companyFiles, JsonRequestBehavior.AllowGet);
		} // Accounts

		[HttpPost]
		public ActionResult UploadedFiles() {
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			OneUploadLimitation oLimitations = CurrentValues.Instance.GetUploadLimitations("CompanyFilesMarketPlaces", "UploadedFiles");

			for (int i = 0; i < Request.Files.Count; ++i) {
				HttpPostedFileBase file = Request.Files[i];

				if (file != null) {
					var content = new byte[file.ContentLength];

					int nRead = file.InputStream.Read(content, 0, file.ContentLength);

					if (nRead != file.ContentLength) {
						Log.Warn("File {0}: failed to read entire file contents, ignoring.", i);
						continue;
					} // if

					string sMimeType = oLimitations.DetectFileMimeType(content, file.FileName, oLog: Log);

					if (string.IsNullOrWhiteSpace(sMimeType)) {
						Log.Warn("Not saving file #{0}: {1} because it has unsupported MIME type.", (i + 1), file.FileName);
						continue;
					} // if

					m_oServiceClient.Instance.CompanyFilesUpload(_context.Customer.Id, file.FileName, content, sMimeType);
				} // if
			} // for

			return Json(new { });
		} // UploadedFiles

		[HttpPost]
		public ActionResult Connect() {
			int mpId = -1;

			try {
				new Transactional(() => {
					var serviceInfo = new CompanyFilesServiceInfo();
					var name = serviceInfo.DisplayName;
					var cf = new CompanyFilesDatabaseMarketPlace();
					var mp = _helper.SaveOrUpdateCustomerMarketplace(_context.Customer.Name + "_" + name, cf, null, _context.Customer);
					var rdh = mp.Marketplace.GetRetrieveDataHelper(_helper);
					rdh.UpdateCustomerMarketplaceFirst(mp.Id);
					mpId = mp.Id;
				}).Execute();

				if (mpId != -1) {
					m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, mpId, true, _context.UserId);
					m_oServiceClient.Instance.MarketplaceInstantUpdate(mpId);
				} // if
			}
			catch (Exception ex) {
				Log.Error(ex, "Error connecting a company files account.");
			} // try

			return Json(new { });
		} // Connect

		private static readonly ASafeLog Log = new SafeILog(typeof(CompanyFilesMarketPlacesController));

		private readonly IEzbobWorkplaceContext _context;
		private readonly Customer _customer;
		private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;
	} // class CompanyFilesMarketPlacesController
} // namespace