﻿namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class AccountSettingsController : Controller {
		#region constructor

		public AccountSettingsController(IWorkplaceContext context) {
			m_oContext = context;
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region action UpdateSecurityQuestion

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UpdateSecurityQuestion(SecurityQuestionModel model, string password) {
			string sErrorMsg = null;
			bool bSuccess = false;

			try {
				StringActionResult sar = m_oServiceClient.Instance.UserUpdateSecurityQuestion(
					m_oContext.User.Name,
					password,
					model.Question,
					model.Answer
				);

				sErrorMsg = sar.Value;

				bSuccess = string.IsNullOrWhiteSpace(sErrorMsg);
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update security question for customer '{0}'.", m_oContext.User.Name);
				sErrorMsg = "Failed to update security question.";
				bSuccess = false;
			} // try

			return Json(new { success = bSuccess, error = sErrorMsg, });
		} // UpdateSecurityQuestion

		#endregion action UpdateSecurityQuestion

		#region action ChangePassword

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangePassword(string oldPassword, string newPassword) {
			string sErrorMsg = null;
			bool bSuccess = false;

			try {
				StringActionResult sar = m_oServiceClient.Instance.CustomerChangePassword(m_oContext.User.Name, oldPassword, newPassword);

				sErrorMsg = sar.Value;

				bSuccess = string.IsNullOrWhiteSpace(sErrorMsg);

				if (bSuccess)
					m_oContext.User.IsPasswordRestored = false;
			}
			catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update password for customer '{0}'.", m_oContext.User.Name);
				sErrorMsg = "Failed to update password.";
				bSuccess = false;
			} // try

			return Json(new { success = bSuccess, error = sErrorMsg, });
		} // ChangePassword

		#endregion action ChangePassword

		#region private

		private readonly IWorkplaceContext m_oContext;
		private readonly ServiceClient m_oServiceClient;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(AccountSettingsController));

		#endregion private
	} // class AccountSettingsController
} // namespace
