namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
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
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class CompanyFilesMarketPlacesController : Controller {
		public CompanyFilesMarketPlacesController(IEzbobWorkplaceContext context, DatabaseDataHelper dbHelper, CompanyFilesMetaDataRepository companyFilesMetaDataRepository) {
			this.context = context;
			this.customer = context.Customer;
			this.dbHelper = dbHelper;
			this.companyFilesMetaDataRepository = companyFilesMetaDataRepository;
			this.serviceClient = new ServiceClient();
		} // constructor

		public JsonResult Accounts() {
			var oEsi = new CompanyFilesServiceInfo();
			bool hasCompanyFilesMp = this.customer.CustomerMarketPlaces.Any(mp => mp.Marketplace.InternalId == oEsi.InternalId);
			if (hasCompanyFilesMp) {
				var companyFiles = this.companyFilesMetaDataRepository.GetFinancialDocumentFiles(this.customer.Id)
					.ToList();

				return Json(companyFiles, JsonRequestBehavior.AllowGet);
			}

			return Json(new List<string>(), JsonRequestBehavior.AllowGet);
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

					this.serviceClient.Instance.CompanyFilesUpload(this.customer.Id, OneUploadLimitation.FixFileName(file.FileName), content, sMimeType, false);
				} // if
			} // for

			return Json(new { });
		} // UploadedFiles

		[HttpPost]
		public ActionResult BankUploadedFiles() {
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

					this.serviceClient.Instance.CompanyFilesUpload(this.customer.Id, OneUploadLimitation.FixFileName(file.FileName), content, sMimeType, true);
				} // if
			} // for

			return Json(new { });
		} // BankUploadedFiles

		[HttpPost]
		public ActionResult Connect() {
			int mpId = -1;

			try {
				new Transactional(() => {
					var serviceInfo = new CompanyFilesServiceInfo();
					var name = serviceInfo.DisplayName;
					var cf = new CompanyFilesDatabaseMarketPlace();
					var mp = this.dbHelper.SaveOrUpdateCustomerMarketplace(this.customer.Name + "_" + name, cf, null, this.customer);
					var rdh = mp.Marketplace.GetRetrieveDataHelper(this.dbHelper);
					rdh.UpdateCustomerMarketplaceFirst(mp.Id);
					mpId = mp.Id;
				}).Execute();

				if (mpId != -1) {
					this.serviceClient.Instance.UpdateMarketplace(this.customer.Id, mpId, true, this.context.UserId);
					this.serviceClient.Instance.MarketplaceInstantUpdate(mpId);
				} // if
			}
			catch (Exception ex) {
				Log.Error(ex, "Error connecting a company files account.");
			} // try

			return Json(new { });
		} // Connect

		private static readonly ASafeLog Log = new SafeILog(typeof(CompanyFilesMarketPlacesController));

		private readonly IEzbobWorkplaceContext context;
		private readonly Customer customer;
		private readonly DatabaseDataHelper dbHelper;
		private readonly ServiceClient serviceClient;
		private readonly CompanyFilesMetaDataRepository companyFilesMetaDataRepository;
	} // class CompanyFilesMarketPlacesController
} // namespace