namespace EzBob.Web.Areas.Broker.Controllers {
	using System;
	using System.Linq;
	using System.Web.Helpers;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using EzBob.Web.Areas.Broker.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EzBob.Web.Infrastructure.Filters;
	using ServiceClientProxy.EzServiceReference;

	public class BrokerAccountController : ABrokerBaseController {
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[CaptchaValidationFilter(Order = 999999)]
		public JsonResult Signup(BrokerSignupModel model) {
			string sReferredBy = "";

			if (Request.Cookies.AllKeys.Contains(Constant.SourceRef)) {
				var oCookie = Request.Cookies[Constant.SourceRef];

				if (oCookie != null)
					sReferredBy = oCookie.Value;
			} // if

			var uio = UiCustomerOrigin.Get();

			ms_oLog.Debug(
				"Broker sign up request:" +
				"\n\tFirm name: {0}" +
				"\n\tFirm reg #: {1}" +
				"\n\tContact person name: {2}" +
				"\n\tContact person email: {3}" +
				"\n\tContact person mobile: {4}" +
				"\n\tMobile code: {5}" +
				"\n\tContact person other phone: {6}" +
				"\n\tEstimated monthly amount: {7}" +
				"\n\tFirm web site URL: {8}" +
				"\n\tEstimated monthly application count: {9}" +
				"\n\tCaptcha enabled: {10}" +
				"\n\tTerms ID: {11}" +
				"\n\tReferred by (source ref): {12}" +
				"\n\tFCARegistered: {13}" +
				"\n\tLicense Number: {14}" +
				"\n\tUI origin: {15}",
				model.FirmName,
				model.FirmRegNum,
				model.ContactName,
				model.ContactEmail,
				model.ContactMobile,
				model.MobileCode,
				model.ContactOtherPhone,
				model.EstimatedMonthlyClientAmount,
				model.FirmWebSite,
				model.EstimatedMonthlyAppCount,
				model.IsCaptchaEnabled == 0 ? "no" : "yes",
				model.TermsID,
				sReferredBy,
				model.FCARegistered,
				model.LicenseNumber,
				uio.CustomerOriginID
			);

			if (!ModelState.IsValid) {
				return new BrokerForJsonResult(string.Join(
					"; ",
					ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)
				));
			} // if

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn(
					"Sign up request with contact email {0}: already authorized as {1}.",
					model.ContactEmail,
					User.Identity.Name
				);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerPropertiesActionResult bp;

			try {
				bp = this.m_oServiceClient.Instance.BrokerSignup(
					model.FirmName,
					model.FirmRegNum,
					model.ContactName,
					model.ContactEmail,
					model.ContactMobile,
					model.MobileCode,
					model.ContactOtherPhone,
					model.EstimatedMonthlyClientAmount,
					new DasKennwort(model.Password), 
					new DasKennwort(model.Password2), 
					model.FirmWebSite,
					model.EstimatedMonthlyAppCount,
					model.IsCaptchaEnabled != 0,
					model.TermsID,
					sReferredBy,
					model.FCARegistered,
					model.LicenseNumber,
					uio.CustomerOriginID
				);

				if (!string.IsNullOrEmpty(bp.Properties.ErrorMsg)) {
					ms_oLog.Warn("Failed to sign up as a broker. {0}", bp.Properties.ErrorMsg);
					return new BrokerForJsonResult(bp.Properties.ErrorMsg);
				} // if
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to sign up as a broker.");
				return new BrokerForJsonResult("Registration failed. Please contact customer care.");
			} // try

			BrokerHelper.SetAuth(model.ContactEmail);

			ms_oLog.Debug("Broker sign up succeeded for: {0}", model.ContactEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp.Properties) {
				antiforgery_token = AntiForgery.GetHtml().ToString()
			};
		} // Signup

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Login(string LoginEmail, string LoginPassword) {
			ms_oLog.Debug("Broker login request: {0}", LoginEmail);

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn(
					"Login request with contact email {0}: already authorized as {1}.",
					LoginEmail,
					User.Identity.Name
				);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			BrokerProperties bp = this.m_oHelper.TryLogin(LoginEmail, LoginPassword, null, null);

			if (bp == null)
				return new BrokerForJsonResult("Failed to log in.");

			ms_oLog.Debug("Broker login succeeded for: {0}", LoginEmail);

			return new PropertiesBrokerForJsonResult(oProperties: bp) {
				antiforgery_token = AntiForgery.GetHtml().ToString()
			};
		} // Login

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Logoff(string sContactEmail) {
			bool bGoodToLogOff =
				string.IsNullOrWhiteSpace(sContactEmail) || (
				User.Identity.IsAuthenticated &&
				(User.Identity.Name == sContactEmail) &&
				(UiOrigin == SessionUiOrigin)
			);

			if (bGoodToLogOff) {
				this.m_oHelper.Logoff(User.Identity.Name, HttpContext);

				return new BrokerForJsonResult {
					antiforgery_token = AntiForgery.GetHtml().ToString(),
				};
			} // if

			ms_oLog.Warn(
				"Log off request with contact email {0} while {1} logged in.",
				sContactEmail,
				User.Identity.IsAuthenticated
					? "broker " + User.Identity.Name + " with origin " + SessionUiOrigin + " is"
					: "not"
			);

			return new BrokerForJsonResult(bExplicitSuccess: false);
		} // Logoff

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RestorePassword(string ForgottenMobile, string ForgottenMobileCode) {
			ms_oLog.Debug(
				"Broker restore password request: phone # {0} with code {1}",
				ForgottenMobile,
				ForgottenMobileCode
			);

			if (User.Identity.IsAuthenticated) {
				ms_oLog.Warn(
					"Request with mobile phone {0} and code {1}: already authorized as {2}.",
					ForgottenMobile,
					ForgottenMobileCode,
					User.Identity.Name
				);
				return new BrokerForJsonResult("You are already logged in.");
			} // if

			try {
				this.m_oServiceClient.Instance.BrokerRestorePassword(ForgottenMobile, ForgottenMobileCode);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to restore password for a broker with phone # {0}.", ForgottenMobile);
				return new BrokerForJsonResult("Failed to restore password.");
			} // try

			ms_oLog.Debug("Broker restore password succeeded for phone # {0}", ForgottenMobile);

			return new BrokerForJsonResult();
		} // RestorePassword

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult UpdatePassword(string ContactEmail, string OldPassword, string NewPassword, string NewPassword2) {
			ms_oLog.Debug("Broker update password request for contact email {0} with origin {1}", ContactEmail, UiOrigin);

			var oIsAuthResult = IsAuth<BrokerForJsonResult>("Update password", ContactEmail);

			if (oIsAuthResult != null)
				return oIsAuthResult;

			bool passwordIsNull =
				ReferenceEquals(OldPassword, null) ||
				ReferenceEquals(NewPassword, null) ||
				ReferenceEquals(NewPassword2, null);

			if (passwordIsNull) {
				ms_oLog.Warn(
					"Cannot update password for contact email {0} with origin {1}: one of passwords not specified.",
					ContactEmail,
					UiOrigin
				);
				return new BrokerForJsonResult("Cannot update password: some required fields are missing.");
			} // if

			if (NewPassword != NewPassword2) {
				ms_oLog.Warn(
					"Cannot update password for contact email {0} with origin {1}: passwords do not match.",
					ContactEmail,
					UiOrigin
				);
				return new BrokerForJsonResult("Cannot update password: passwords do not match.");
			} // if

			if (NewPassword == OldPassword) {
				ms_oLog.Warn(
					"Cannot update password for contact email {0} with origin {1}: new password is equal to the old one.",
					ContactEmail,
					UiOrigin
				);
				return new BrokerForJsonResult("Cannot update password: new password is equal to the old one.");
			} // if

			ActionMetaData oResult;

			try {
				oResult = this.m_oServiceClient.Instance.BrokerUpdatePassword(
					ContactEmail,
					UiOrigin,
					new DasKennwort(OldPassword),
					new DasKennwort(NewPassword),
					new DasKennwort(NewPassword2)
				);
			} catch (Exception e) {
				ms_oLog.Alert(e, "Failed to update password for contact email {0} with origin {1}.", ContactEmail, UiOrigin);
				return new BrokerForJsonResult("Failed to update password.");
			} // try

			if (oResult == null) {
				ms_oLog.Warn("Failed to update password for contact email {0} with origin {1}.", ContactEmail, UiOrigin);
				return new BrokerForJsonResult("Failed to update password.");
			} // if

			ms_oLog.Debug(
				"Broker update password request for contact email {0} with origin {1} complete.",
				ContactEmail,
				UiOrigin
			);

			return new BrokerForJsonResult();
		} // UpdatePassword
	} // class BrokerAccountController
} // namespace
