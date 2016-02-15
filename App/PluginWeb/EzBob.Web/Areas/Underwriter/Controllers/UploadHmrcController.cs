namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Web.Mvc;
	using Customer.Controllers;
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
	} // class UploadHmrcController
} // namespace
