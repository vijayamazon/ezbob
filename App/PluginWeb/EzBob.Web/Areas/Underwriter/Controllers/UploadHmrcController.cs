namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Web.Mvc;
	using Customer.Controllers;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Hmrc;
	using NHibernate;

	public class UploadHmrcController : Controller {
		#region constructor

		public UploadHmrcController(
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session,
			CustomerRepository customers
		) {
			m_oAccountManager = new HmrcManualAccountManager(customers, helper, mpTypes, mpChecker, session);
		} // constructor

		#endregion constructor

		#region action SaveFile

		[HttpPost]
		public JsonResult SaveFile() {
			int nCustomerID;

			if (!int.TryParse(Request.Headers["ezbob-underwriter-customer-id"], out nCustomerID))
				return HmrcManualAccountManager.CreateJsonError("Failed to upload files: customer id header is missing.");

			return m_oAccountManager.SaveUploadedFiles(Request.Files, nCustomerID, "UploadHmrcController", "SaveFile");
		} // SaveFile

		#endregion action SaveFile

		#region action SaveNewManuallyEntered

		[HttpPost]
		public JsonResult SaveNewManuallyEntered(string sData) {
			return m_oAccountManager.SaveNewManuallyEntered(sData);
		} // SaveNewManuallyEntered

		#endregion action SaveNewManuallyEntered

		#region action LoadPeriods

		[HttpGet]
		public JsonResult LoadPeriods([Bind(Prefix = "customerId")] int nCustomerID) {
			return m_oAccountManager.LoadPeriods(nCustomerID);
		} // LoadPeriods

		#endregion action LoadPeriods

		#region action RemovePeriod

		[HttpPost]
		public JsonResult RemovePeriod(string period) {
			return m_oAccountManager.RemovePeriod(period);
		} // RemovePeriod

		#endregion action RemovePeriod

		#region private

		private readonly HmrcManualAccountManager m_oAccountManager;

		#endregion private
	} // class UploadHmrcController
} // namespace
