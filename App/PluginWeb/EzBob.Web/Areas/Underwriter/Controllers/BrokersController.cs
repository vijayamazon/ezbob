namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class BrokersController : Controller {
		#region constructor

		public BrokersController(IEzbobWorkplaceContext context) {
			m_oServiceClient = new ServiceClient();
			m_oContext = context;
		} // constructor

		#endregion constructor

		#region action LoadCustomers

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult LoadCustomers(int nBrokerID) {
			ms_oLog.Debug("Load broker customers request for broker {0}...", nBrokerID);

			BrokerCustomersActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadCustomersByID(nBrokerID);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Loading broker customers request for broker {0} failed.", nBrokerID);
				return Json(new { aaData = new BrokerCustomerEntry[0], }, JsonRequestBehavior.AllowGet);
			} // try

			ms_oLog.Debug("Load broker customers request for broker {0} complete.", nBrokerID);
			return Json(new { aaData = oResult.Customers, }, JsonRequestBehavior.AllowGet);
		} // LoadCustomers

		#endregion action LoadCustomers

		#region action LoadProperties

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadProperties(int nBrokerID) {
			ms_oLog.Debug("Load broker properties request for broker {0}...", nBrokerID);

			BrokerPropertiesActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadPropertiesByID(nBrokerID);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Load broker properties request for broker {0} failed.", nBrokerID);
				return Json(new BrokerProperties(), JsonRequestBehavior.AllowGet);
			} // try

			ms_oLog.Debug("Load broker properties request for broker {0} complete.", nBrokerID);

			return Json(oResult.Properties, JsonRequestBehavior.AllowGet);
		} // LoadProperties

		#endregion action LoadProperties

		#region action LoadBrokerID2UserIDEntry

		// TODO: remove this once EZ-2459 is implemented

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadBrokerID2UserIDEntry(int nBrokerID) {
			int nUserID = 0;

			ms_oLog.Debug("Load broker's user ID request for broker {0}...", nBrokerID);

			Broker oBroker = ObjectFactory.GetInstance<BrokerRepository>().GetAll().FirstOrDefault(b => b.ID == nBrokerID);

			if (oBroker != null)
				nUserID = oBroker.UserID;

			ms_oLog.Debug("Load broker's user ID request for broker {0} complete.", nBrokerID);

			return Json(new { userID = nUserID }, JsonRequestBehavior.AllowGet);
		} // LoadBrokerID2UserIDEntry

		#endregion action LoadBrokerID2UserIDEntry

		#region action ResetPassword123456

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ResetPassword123456(int nBrokerID) {
			new ServiceClient().Instance.ResetPassword123456(m_oContext.User.Id, nBrokerID, PasswordResetTarget.Broker);
			return Json(true);
		} // ResetPassword123456

		#endregion action ResetPassword123456

		#region private

		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(BrokersController));

		#endregion private
	} // class BrokersController
} // namespace
