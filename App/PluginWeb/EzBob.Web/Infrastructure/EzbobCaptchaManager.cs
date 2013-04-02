using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using CaptchaMvc.Infrastructure;
using CaptchaMvc.Interface;

namespace EzBob.Web.Infrastructure
{
    public class EzbobCaptchaManager : DefaultCaptchaManager 
    {
        protected override string GenerateImageUrl(UrlHelper urlHelper, KeyValuePair<string, ICaptchaValue> captchaPair)
        {
            return urlHelper.Action("Generate", "DefaultCaptcha",
                new RouteValueDictionary { { TokenParameterName, captchaPair.Key }, {"Area", ""} });
        }
    }

}