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
			m_oAccountManager = new HmrcManualAccountManager(
				true,
				customers,
				helper,
				mpTypes,
				mpChecker,
				context
			);

			m_oContext = context;
		} // constructor

		[HttpPost]
		public JsonResult SaveFile() {
			int nCustomerID = DetectCustomer();

			if (nCustomerID <= 0)
				return HmrcManualAccountManager.CreateJsonError("Please log out and log in again.");

			return m_oAccountManager.SaveUploadedFiles(Request.Files, nCustomerID, "HmrcController", "SaveFile");
		} // SaveFile

		[HttpGet]
		public JsonResult LoadPeriods() {
			return m_oAccountManager.LoadPeriods(DetectCustomer());
		} // LoadPeriods

		private int DetectCustomer() {
			int nCustomerID;

			try {
				nCustomerID = m_oContext.Customer.Id;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to fetch current customer.");
				nCustomerID = 0;
			} // try

			return nCustomerID;
		} // DetectCustomer

		private readonly HmrcManualAccountManager m_oAccountManager;
		private readonly IEzbobWorkplaceContext m_oContext;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(HmrcController));
	} // class HmrcController
} // namespace
