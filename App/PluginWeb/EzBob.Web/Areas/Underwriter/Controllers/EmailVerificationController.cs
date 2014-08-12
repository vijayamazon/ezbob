namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class EmailVerificationController : Controller {
		public EmailVerificationController(IEzbobWorkplaceContext oContext) {
			m_oServiceClient = new ServiceClient();
			m_oContext = oContext;
		} // constructor

		[HttpPost]
		[Permission(Name = "EmailConfirmationButton")]
		public JsonResult ManuallyConfirm(int id) {
			try {
				m_oServiceClient.Instance.EmailConfirmationConfirmUser(id, m_oContext.User.Id);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to generate and send email confirmation request.");
			} // try
			return Json(new { });
		} // ManuallyConfirm

		[Permission(Name = "EmailConfirmationButton")]
		[HttpPost]
		public JsonResult Resend(int id) {
			try {
				m_oServiceClient.Instance.EmailConfirmationGenerateAndSend(id);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to generate and send email confirmation request to customer {0}.", id);
			} // try

			return Json(new { });
		} // Resend

		[HttpPost]
		[Permission(Name = "EmailConfirmationButton")]
		public JsonResult ChangeEmail(int id, string email) {
			string sErrorMessage;

			try {
				StringActionResult sar = m_oServiceClient.Instance.UserChangeEmail(id, email);
				sErrorMessage = sar.Value;
			}
			catch (Exception e) {
				sErrorMessage = "Failed to change a user email.";
				ms_oLog.Alert(e, sErrorMessage);
			} // try

			return Json(new { success = string.IsNullOrWhiteSpace(sErrorMessage), error = sErrorMessage, });
		} // ChangeEmail

		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext m_oContext;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(EmailVerificationController));
	} // class EmailVerificationController
} // namespace
