namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Web;
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
		public BrokersController(IEzbobWorkplaceContext context) {
			m_oServiceClient = new ServiceClient();
			m_oContext = context;
		} // constructor

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult LoadCustomers(int nBrokerID) {
			ms_oLog.Debug("Load broker customers request for broker {0}...", nBrokerID);

			BrokerCustomersActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadCustomersByID(nBrokerID);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Loading broker customers request for broker {0} failed.", nBrokerID);
				return Json(new { aaData = new BrokerCustomerEntry[0], }, JsonRequestBehavior.AllowGet);
			} // try

			ms_oLog.Debug("Load broker customers request for broker {0} complete.", nBrokerID);
			return Json(new { aaData = oResult.Customers, }, JsonRequestBehavior.AllowGet);
		} // LoadCustomers

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadProperties(int nBrokerID) {
			ms_oLog.Debug("Load broker properties request for broker {0}...", nBrokerID);

			BrokerPropertiesActionResult oResult;

			try {
				oResult = m_oServiceClient.Instance.BrokerLoadPropertiesByID(nBrokerID);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Load broker properties request for broker {0} failed.", nBrokerID);
				return Json(new BrokerProperties(), JsonRequestBehavior.AllowGet);
			} // try

			ms_oLog.Debug("Load broker properties request for broker {0} complete.", nBrokerID);

			return Json(oResult.Properties, JsonRequestBehavior.AllowGet);
		} // LoadProperties

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		[Permission(Name = "ResetBrokerPassword")]
		public JsonResult ResetPassword123456(int nBrokerID) {
			try {
				new ServiceClient().Instance.ResetPassword123456(m_oContext.User.Id, nBrokerID, PasswordResetTarget.Broker);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to reset broker {0}'s password to 123456.", nBrokerID);
			} // try

			return Json(true);
		} // ResetPassword123456

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		[Permission(Name = "ChangeBrokerEmail")]
		public JsonResult UpdateEmail(int brokerID, string newEmail) {
			if (string.IsNullOrWhiteSpace(newEmail))
				return Json(new { success = false, error = "email address is empty", });
			try {
				StringActionResult res = new ServiceClient().Instance.BrokerUpdateEmail(
					m_oContext.User.Id,
					brokerID,
					newEmail
				);

				return Json(new { success = string.IsNullOrWhiteSpace(res.Value), error = res.Value, });
			} catch (Exception e) {
				string error = string.Format("failed to update broker {0}'s email to '{1}'", brokerID, newEmail);

				ms_oLog.Alert(e, "Not good: {0}.", error);

				return Json(new { success = false, error = error, });
			} // try
		} // UpdateEmail

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		[Permission(Name = "ChangeBroker")]
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

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult LoadWhiteLabel(int nBrokerID) {
			ms_oLog.Debug("Load broker white label request for broker {0}...", nBrokerID);

			var brokerRepo = ObjectFactory.GetInstance<BrokerRepository>();
			var broker = brokerRepo.Get(nBrokerID);
			if (broker != null)
				return Json(new { WhiteLabel = broker.WhiteLabel ?? new WhiteLabelProvider() }, JsonRequestBehavior.AllowGet);

			return Json(new { error = "broker not found" });
		} // LoadCustomers

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "BrokerWhiteLabel")]
		public JsonResult SaveWhiteLabel(int brokerId, WhiteLabelProvider whiteLabel) {
			ms_oLog.Debug("Save broker white label request for broker {0}...", brokerId);

			var whiteLabelRepo = ObjectFactory.GetInstance<WhiteLabelProviderRepository>();

			if (whiteLabelRepo.GetByName(whiteLabel.Name) != null)
				return Json(new { error = "white label with such name already exists" });

			var brokerRepo = ObjectFactory.GetInstance<BrokerRepository>();

			var broker = brokerRepo.Get(brokerId);

			if (broker != null) {
				whiteLabel.LogoWidthPx = 128;
				whiteLabel.LogoHeightPx = 34;
				broker.WhiteLabel = whiteLabel;
				return Json(new { broker.WhiteLabel }, JsonRequestBehavior.AllowGet);
			} // if

			return Json(new { error = "broker not found" });
		} // SaveWhiteLabel 

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult UpdateWhiteLabel(int whiteLabelId, WhiteLabelProvider whiteLabel) {
			ms_oLog.Debug("Update broker white label request for white label {0}...", whiteLabelId);

			var whiteLabelRepo = ObjectFactory.GetInstance<WhiteLabelProviderRepository>();
			var wl = whiteLabelRepo.Get(whiteLabelId);

			if (wl == null)
				return Json(new { error = "white label not found" });

			wl.Logo = whiteLabel.Logo ?? wl.Logo;
			wl.LogoImageType = whiteLabel.LogoImageType ?? wl.LogoImageType;
			wl.Name = whiteLabel.Name ?? wl.Name;
			wl.Phone = whiteLabel.Phone ?? wl.Phone;
			wl.LeadingColor = whiteLabel.LeadingColor ?? wl.LeadingColor;
			wl.SecondoryColor = whiteLabel.SecondoryColor ?? wl.SecondoryColor;
			wl.Email = whiteLabel.Email ?? wl.Email;
			wl.FinishWizardText = whiteLabel.FinishWizardText ?? wl.FinishWizardText;
			wl.FooterText = whiteLabel.FooterText ?? wl.FooterText;
			wl.ConnectorsToEnable = whiteLabel.ConnectorsToEnable ?? wl.ConnectorsToEnable;

			whiteLabelRepo.Update(wl);
			return Json(new { success = true });
		} // UpdateWhiteLabel 

		[HttpPost]
		public JsonResult UploadLogo() {
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			if (Request.Files.Count != 1)
				return Json(new { error = "Only one logo upload supported" });

			HttpPostedFileBase file = Request.Files[0];
			if (file != null) {
				var content = new byte[file.ContentLength];
				file.InputStream.Read(content, 0, file.ContentLength);
				return Json(new { Logo = Convert.ToBase64String(content), LogoType = file.ContentType });
			} // if

			return Json(new { error = "broker not found" });
		} // UploadLogo

		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(BrokersController));
	} // class BrokersController
} // namespace
