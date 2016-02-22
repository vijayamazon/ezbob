namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Collections.Generic;
	using System.Web.Mvc;
	using Customer.Controllers;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using Infrastructure.Hmrc;

	public class UploadHmrcController : Controller {
		public UploadHmrcController(
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			CustomerRepository customers,
			IWorkplaceContext context
		) {
			this.vatAccountManager = new HmrcManualAccountManager(
				false,
				customers,
				helper,
				mpTypes,
				mpChecker,
				context
			);
		} // constructor

		[HttpPost]
		[Permission(Name = "EnterHMRC")]
		public JsonResult SaveFile() {
			log.Debug("Uploading {0} from UW.", Grammar.Number(Request.Files.Count, "file"));

			if (Request.Files.Count > 0) {
				var lst = new List<string>();

				foreach (string f in Request.Files)
					lst.Add(f);

				log.Debug("File name{0}:\n\t{1}", Request.Files.Count == 1 ? " is" : "s are", string.Join("\n\t", lst));
			} // if

			int nCustomerID;

			if (!int.TryParse(Request.Headers["ezbob-underwriter-customer-id"], out nCustomerID))
				return HmrcManualAccountManager.CreateJsonError("Failed to upload files: customer id header is missing.");

			return this.vatAccountManager.SaveUploadedFiles(Request.Files, nCustomerID, "UploadHmrcController", "SaveFile");
		} // SaveFile

		[HttpPost]
		[Permission(Name="EnterHMRC")]
		public JsonResult SaveNewManuallyEntered(string sData) {
			return this.vatAccountManager.SaveNewManuallyEntered(sData);
		} // SaveNewManuallyEntered

		[HttpGet]
		public JsonResult LoadPeriods([Bind(Prefix = "customerId")] int nCustomerID) {
			return this.vatAccountManager.LoadPeriods(nCustomerID);
		} // LoadPeriods

		[HttpPost]
		[Permission(Name = "EnterHMRC")]
		public JsonResult RemovePeriod(string period) {
			return this.vatAccountManager.RemovePeriod(period);
		} // RemovePeriod

		private readonly HmrcManualAccountManager vatAccountManager;

		private static readonly ASafeLog log = new SafeILog(typeof(UploadHmrcController));
	} // class UploadHmrcController
} // namespace
