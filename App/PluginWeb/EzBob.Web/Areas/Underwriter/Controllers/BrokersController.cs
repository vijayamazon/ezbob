namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class BrokersController : Controller {
		#region constructor

		public BrokersController() {
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region action LoadCustomers

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult LoadCustomers(int nBrokerID) {
			ms_oLog.Debug("Loading broker customers request for broker {0}...", nBrokerID);

			BrokerCustomersActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadCustomersByID(nBrokerID);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Loading broker customers request for broker {0} failed.", nBrokerID);
				return Json(new { aaData = new BrokerCustomerEntry[0], }, JsonRequestBehavior.AllowGet);
			} // try

			ms_oLog.Debug("Loading broker customers request for broker {0} complete.", nBrokerID);
			return Json(new { aaData = oResult.Customers, }, JsonRequestBehavior.AllowGet);
		} // LoadCustomers

		#endregion action LoadCustomers

		#region private

		private readonly ServiceClient m_oServiceClient;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(BrokersController));

		#endregion private
	} // class BrokersController
} // namespace
