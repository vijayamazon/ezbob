using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using Recaptcha;
using StructureMap;

namespace EzBob.Web.Infrastructure.Filters
{
    struct CapthcaModel
    {
        public string ErrorMessage { get; set; }
        public bool IsValid { get; set; }
    }

    public class CaptchaValidationFilter : ActionFilterAttribute
    {
        private const string ChallengeFieldKey = "recaptcha_challenge_field";
        private const string ResponseFieldKey = "recaptcha_response_field";
        private const string PrivateKey = "6Le8aM8SAAAAAGGFIOwlLu23_L-fndyQN8vVKOUX";

        private CapthcaModel _capthcaModel;
        private readonly IEzBobConfiguration _config;
        private const string ErrorMessage = "Captcha verification code you entered does not match. Please try again";

        public CaptchaValidationFilter()
        {
            _capthcaModel = new CapthcaModel
                {
                    ErrorMessage = ErrorMessage,
                    IsValid = false
                };
            _config = ObjectFactory.GetInstance<IEzBobConfiguration>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
	        if (_config.TwilioEnabled)
	        {
		        _capthcaModel.IsValid = true;
	        }
	        else
	        {

		        switch (_config.CaptchaMode)
		        {
			        case "off":
				        _capthcaModel.IsValid = true;
				        break;
			        case "simple":
				        _capthcaModel.IsValid = CaptchaHelper.IsCaptchaVerify(filterContext.Controller, ErrorMessage);
				        break;
			        case "reCaptcha":
				        var validator = new RecaptchaValidator
					        {
						        PrivateKey = PrivateKey,
						        RemoteIP = filterContext.HttpContext.Request.UserHostAddress,
						        Challenge = filterContext.HttpContext.Request.Form[ChallengeFieldKey],
						        Response = filterContext.HttpContext.Request.Form[ResponseFieldKey]
					        };
				        _capthcaModel.IsValid = validator.Validate().IsValid;
				        break;
		        }
	        }
	        filterContext.Controller.ViewData.ModelState.Clear();
            if (!_capthcaModel.IsValid)
            {
                filterContext.Controller.ViewData.ModelState.AddModelError("", _capthcaModel.ErrorMessage);
            }

            base.OnActionExecuting(filterContext);           
        }
    }
}