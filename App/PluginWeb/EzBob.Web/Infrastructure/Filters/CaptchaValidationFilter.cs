namespace EzBob.Web.Infrastructure.Filters
{
	using System.Web.Mvc;
	using CaptchaMvc.HtmlHelpers;
	using Recaptcha;
	using StructureMap;
	using System;

    public class CaptchaValidationFilter : ActionFilterAttribute
    {
        private const string ChallengeFieldKey = "recaptcha_challenge_field";
        private const string ResponseFieldKey = "recaptcha_response_field";
        private const string PrivateKey = "6Le8aM8SAAAAAGGFIOwlLu23_L-fndyQN8vVKOUX";
	    private const string ErrorMessage = "Captcha verification code you entered does not match. Please try again";

	    private bool isValid;
        private readonly IEzBobConfiguration config;

        public CaptchaValidationFilter()
        {
	        isValid = false;
            config = ObjectFactory.GetInstance<IEzBobConfiguration>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
			if (filterContext.HttpContext.Session != null &&
				filterContext.ActionDescriptor.ActionName == "SignUp" &&
				Convert.ToBoolean(filterContext.HttpContext.Session["IsSmsValidationActive"]) && 
				!Convert.ToBoolean(filterContext.HttpContext.Session["SwitchedToCaptcha"]))
			{
				isValid = true;
			}
	        else
	        {
		        switch (config.CaptchaMode)
		        {
			        case "off":
				        isValid = true;
				        break;
			        case "simple":
						isValid = filterContext.Controller.IsCaptchaVerify(ErrorMessage);
				        break;
			        case "reCaptcha":
				        var validator = new RecaptchaValidator
					        {
						        PrivateKey = PrivateKey,
						        RemoteIP = filterContext.HttpContext.Request.UserHostAddress,
						        Challenge = filterContext.HttpContext.Request.Form[ChallengeFieldKey],
						        Response = filterContext.HttpContext.Request.Form[ResponseFieldKey]
					        };
				        isValid = validator.Validate().IsValid;
				        break;
		        }
	        }
	        filterContext.Controller.ViewData.ModelState.Clear();
            if (!isValid)
            {
				filterContext.Controller.ViewData.ModelState.AddModelError("", ErrorMessage);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}