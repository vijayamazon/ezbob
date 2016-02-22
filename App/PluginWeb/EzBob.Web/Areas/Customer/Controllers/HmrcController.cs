namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Hmrc;

	public class HmrcController : Controller {
		public HmrcController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			CustomerRepository customers
		) {
			this.accountManager = new HmrcManualAccountManager(
				true,
				customers,
				helper,
				mpTypes,
				mpChecker,
				context
			);

			this.context = context;
		} // constructor

		[HttpPost]
		public JsonResult SaveFile() {
			int nCustomerID = DetectCustomer();

			if (nCustomerID <= 0)
				return HmrcManualAccountManager.CreateJsonError("Please log out and log in again.");

			return this.accountManager.SaveUploadedFiles(Request.Files, nCustomerID, "HmrcController", "SaveFile");
		} // SaveFile

		[HttpGet]
		public JsonResult LoadPeriods() {
			return this.accountManager.LoadPeriods(DetectCustomer());
		} // LoadPeriods

		private int DetectCustomer() {
			int nCustomerID;

			try {
				nCustomerID = this.context.Customer.Id;
			}
			catch (Exception e) {
				log.Warn(e, "Failed to fetch current customer.");
				nCustomerID = 0;
			} // try

			return nCustomerID;
		} // DetectCustomer

		private readonly HmrcManualAccountManager accountManager;
		private readonly IEzbobWorkplaceContext context;

		private static readonly ASafeLog log = new SafeILog(typeof(HmrcController));
	} // class HmrcController
} // namespace
