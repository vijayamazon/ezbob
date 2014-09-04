namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
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

		#region action ResetPassword123456

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ResetPassword123456(int nBrokerID) {
			try {
				new ServiceClient().Instance.ResetPassword123456(m_oContext.User.Id, nBrokerID, PasswordResetTarget.Broker);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to reset broker {0}'s password to 123456.", nBrokerID);
			} // try

			return Json(true);
		} // ResetPassword123456

		#endregion action ResetPassword123456

		#region action AttachCustomer

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult AttachCustomer(int nCustomerID, int nBrokerID) {
			try {
				m_oServiceClient.Instance.BrokerAttachCustomer(nCustomerID, nBrokerID <= 0 ? (int?)null : nBrokerID, m_oContext.User.Id);
				return Json(new { success = true, error = string.Empty, });
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update customer's broker for customer {0}, broker {1}.", nCustomerID, nBrokerID);
				return Json(new { success = false, error = e.Message, });
			} // try
		} // AttachCustomer

		#endregion action AttachCustomer

		#region action LoadWhiteLabel

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult LoadWhiteLabel(int nBrokerID) {
			ms_oLog.Debug("Load broker white label request for broker {0}...", nBrokerID);

			var brokerRepo = ObjectFactory.GetInstance<BrokerRepository>();
			var broker = brokerRepo.Get(nBrokerID);
			if (broker != null) {
				return Json(new {WhiteLabel = broker.WhiteLabel ?? new WhiteLabelProvider()}, JsonRequestBehavior.AllowGet);
			}

			return Json(new {error = "broker not found"});
		} // LoadCustomers

		#endregion action LoadWhiteLabel

		#region action SaveWhiteLabel

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveWhiteLabel(int nBrokerID, WhiteLabelProvider whiteLabel) {
			ms_oLog.Debug("Load broker white label request for broker {0}...", nBrokerID);

			var brokerRepo = ObjectFactory.GetInstance<BrokerRepository>();
			var broker = brokerRepo.Get(nBrokerID);
			if (broker != null) {
				broker.WhiteLabel = whiteLabel;
				return Json(new { broker.WhiteLabel }, JsonRequestBehavior.AllowGet);
			}

			return Json(new { error = "broker not found" });
		} // LoadCustomers

		#endregion action SaveWhiteLabel


		#region private

		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(BrokersController));

		#endregion private
	} // class BrokersController
} // namespace
