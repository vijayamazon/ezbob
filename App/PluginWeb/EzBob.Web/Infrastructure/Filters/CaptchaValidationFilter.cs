namespace EzBob.Web.Infrastructure.Filters {
	using System.Web.Mvc;
	using CaptchaMvc.HtmlHelpers;
	using ConfigManager;
	using Recaptcha;
	using log4net;

	public class CaptchaValidationFilter : ActionFilterAttribute {
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			ILog log = LogManager.GetLogger(typeof (CaptchaValidationFilter));

			bool isValid = false;

			bool bSignupSkipCaptcha =
				(filterContext.ActionDescriptor.ActionName == "SignUp") &&
				(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Account") && (
					!filterContext.ActionParameters.ContainsKey("isInCaptchaMode") ||
					(filterContext.ActionParameters["isInCaptchaMode"].ToString() != "True")
				);

			log.DebugFormat("sign up skip captcha: {0}", bSignupSkipCaptcha ? "yes" : "no");

			bool bBrokerSignupSkipCaptcha = false;

			if (!bSignupSkipCaptcha) {
				log.DebugFormat("sign up skip captcha: {0}", bSignupSkipCaptcha ? "yes" : "no");

				bBrokerSignupSkipCaptcha =
					(filterContext.ActionDescriptor.ActionName == "Signup") &&
                    (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "BrokerAccount") && (
						!filterContext.ActionParameters.ContainsKey("IsCaptchaEnabled") ||
						(filterContext.ActionParameters["IsCaptchaEnabled"].ToString() != "1")
					);

				log.DebugFormat("broker sign up skip captcha: {0}", bBrokerSignupSkipCaptcha ? "yes" : "no");
			} // if

			if (bSignupSkipCaptcha || bBrokerSignupSkipCaptcha)
				isValid = true;
			else
			{
				switch (CurrentValues.Instance.CaptchaMode.Value)
				{
				case "off":
					log.Debug("Captcha off mode");
					isValid = true;
					break;

				case "simple":
					log.Debug("Simple Captcha mode");
					isValid = filterContext.Controller.IsCaptchaValid(ErrorMessage);
					break;

				case "reCaptcha":
					log.Debug("re Captcha mode");

					var validator = new RecaptchaValidator {
						PrivateKey = PrivateKey,
						RemoteIP = filterContext.HttpContext.Request.UserHostAddress,
						Challenge = filterContext.HttpContext.Request.Form[ChallengeFieldKey],
						Response = filterContext.HttpContext.Request.Form[ResponseFieldKey]
					};
					isValid = validator.Validate().IsValid;
					break;
				}
			} // if

			filterContext.Controller.ViewData.ModelState.Clear();

			log.DebugFormat("captcha is {0}", isValid ? "valid" : "invalid");

			if (!isValid)
				filterContext.Controller.ViewData.ModelState.AddModelError("", ErrorMessage);

			base.OnActionExecuting(filterContext);
		} // OnActionExecuting

		private const string ChallengeFieldKey = "recaptcha_challenge_field";
		private const string ResponseFieldKey = "recaptcha_response_field";
		private const string PrivateKey = "6Le8aM8SAAAAAGGFIOwlLu23_L-fndyQN8vVKOUX";
		private const string ErrorMessage = "Captcha verification code you entered does not match. Please try again";
	} // class CaptchaValidationFilter
} // namespace
