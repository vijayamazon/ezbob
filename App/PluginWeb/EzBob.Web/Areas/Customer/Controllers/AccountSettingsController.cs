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

	public class AccountSettingsController : Controller {
		public AccountSettingsController(IWorkplaceContext context) {
			this.context = context;
			this.serviceClient = new ServiceClient();
		} // constructor

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangePassword(string oldPassword, string newPassword) {
			string sErrorMsg = null;
			bool bSuccess = false;

			try {
				StringActionResult sar = this.serviceClient.Instance.CustomerChangePassword(
					this.context.User.Name,
					UiCustomerOrigin.Get().GetOrigin(),
					new DasKennwort(oldPassword),
					new DasKennwort(newPassword)
				);

				sErrorMsg = sar.Value;

				bSuccess = string.IsNullOrWhiteSpace(sErrorMsg);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update password for customer '{0}'.", this.context.User.Name);
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
				StringActionResult sar = this.serviceClient.Instance.UserUpdateSecurityQuestion(
					this.context.User.Name,
					UiCustomerOrigin.Get().GetOrigin(),
					new DasKennwort(password),
					model.Question,
					model.Answer
				);

				sErrorMsg = sar.Value;

				bSuccess = string.IsNullOrWhiteSpace(sErrorMsg);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update security question for customer '{0}'.", this.context.User.Name);
				sErrorMsg = "Failed to update security question.";
				bSuccess = false;
			} // try

			return Json(new { success = bSuccess, error = sErrorMsg, });
		} // UpdateSecurityQuestion

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(AccountSettingsController));
		private readonly IWorkplaceContext context;
		private readonly ServiceClient serviceClient;
	} // class AccountSettingsController
} // namespace
