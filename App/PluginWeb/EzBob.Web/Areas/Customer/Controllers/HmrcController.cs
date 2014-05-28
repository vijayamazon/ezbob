namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Hmrc;
	using NHibernate;

	public class HmrcController : Controller {
		#region public

		#region constructor

		public HmrcController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			CGMPUniqChecker mpChecker,
			ISession session,
			CustomerRepository customers
		) {
			m_oAccountManager = new HmrcManualAccountManager(customers, helper, mpTypes, mpChecker, session);
			m_oContext = context;
		} // constructor

		#endregion constructor

		#region action SaveFile

		[HttpPost]
		public JsonResult SaveFile() {
			int nCustomerID;

			try {
				nCustomerID = m_oContext.Customer.Id;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to fetch current customer.");
				return HmrcManualAccountManager.CreateJsonError("Please log out and log in again.");
			} // try

			return m_oAccountManager.SaveUploadedFiles(Request.Files, nCustomerID);
		} // SaveFile

		#endregion action SaveFile

		#endregion public

		#region private

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(HmrcController));

		private readonly HmrcManualAccountManager m_oAccountManager;
		private readonly IEzbobWorkplaceContext m_oContext;

		#endregion private
	} // class HmrcController
} // namespace
