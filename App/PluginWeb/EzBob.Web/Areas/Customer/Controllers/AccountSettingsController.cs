namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using EzBob.Web.Areas.Customer.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	using RemoteCustomerOriginEnum = ServiceClientProxy.EzServiceReference.CustomerOriginEnum;

	public class AccountSettingsController : Controller {
		public AccountSettingsController(IWorkplaceContext context) {
			m_oContext = context;
			m_oServiceClient = new ServiceClient();
		} // constructor

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangePassword(string oldPassword, string newPassword) {
			string sErrorMsg = null;
			bool bSuccess = false;

			try {
				StringActionResult sar = m_oServiceClient.Instance.CustomerChangePassword(
					m_oContext.User.Name,
					(RemoteCustomerOriginEnum)(int)UiCustomerOrigin.Get().GetOrigin(),
					new DasKennwort(oldPassword),
					new DasKennwort(newPassword)
				);

				sErrorMsg = sar.Value;

				bSuccess = string.IsNullOrWhiteSpace(sErrorMsg);

				if (bSuccess)
					m_oContext.User.IsPasswordRestored = false;
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update password for customer '{0}'.", m_oContext.User.Name);
				sErrorMsg = "Failed to update password.";
				bSuccess = false;
			} // try

			return Json(new { success = bSuccess, error = sErrorMsg, });
		} // ChangePassword

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UpdateSecurityQuestion(SecurityQuestionModel model, string password) {
			string sErrorMsg = null;
			bool bSuccess = false;

			try {
				StringActionResult sar = m_oServiceClient.Instance.UserUpdateSecurityQuestion(
					m_oContext.User.Name,
					new Password(password),
					model.Question,
					model.Answer
				);

				sErrorMsg = sar.Value;

				bSuccess = string.IsNullOrWhiteSpace(sErrorMsg);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update security question for customer '{0}'.", m_oContext.User.Name);
				sErrorMsg = "Failed to update security question.";
				bSuccess = false;
			} // try

			return Json(new { success = bSuccess, error = sErrorMsg, });
		} // UpdateSecurityQuestion

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(AccountSettingsController));
		private readonly IWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;
	} // class AccountSettingsController
} // namespace
