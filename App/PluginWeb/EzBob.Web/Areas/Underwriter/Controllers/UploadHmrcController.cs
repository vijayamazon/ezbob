namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Web.Mvc;
	using Customer.Controllers;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using Infrastructure.Hmrc;
	using NHibernate;

	public class UploadHmrcController : Controller {

		public UploadHmrcController(
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session,
			CustomerRepository customers, IWorkplaceContext context) {
			m_oAccountManager = new HmrcManualAccountManager(customers, helper, mpTypes, mpChecker, session, context);
		}

		// constructor

		[HttpPost]
		public JsonResult SaveFile() {
			int nCustomerID;

			if (!int.TryParse(Request.Headers["ezbob-underwriter-customer-id"], out nCustomerID))
				return HmrcManualAccountManager.CreateJsonError("Failed to upload files: customer id header is missing.");

			return m_oAccountManager.SaveUploadedFiles(Request.Files, nCustomerID, "UploadHmrcController", "SaveFile");
		} // SaveFile

		[HttpPost]
		public JsonResult SaveNewManuallyEntered(string sData) {
			return m_oAccountManager.SaveNewManuallyEntered(sData);
		} // SaveNewManuallyEntered

		[HttpGet]
		public JsonResult LoadPeriods([Bind(Prefix = "customerId")] int nCustomerID) {
			return m_oAccountManager.LoadPeriods(nCustomerID);
		} // LoadPeriods

		[HttpPost]
		public JsonResult RemovePeriod(string period) {
			return m_oAccountManager.RemovePeriod(period);
		} // RemovePeriod

		private readonly HmrcManualAccountManager m_oAccountManager;

	} // class UploadHmrcController
} // namespace
