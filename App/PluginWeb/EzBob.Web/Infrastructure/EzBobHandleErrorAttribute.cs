using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;

namespace EzBob.Web.Infrastructure
{

    public class EzBobHandleErrorAttribute:HandleErrorAttribute
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(EzBobHandleErrorAttribute));
        private string _errorMessage;

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null) return;

            ThreadContext.Properties["Action"] = filterContext.RouteData.Values["action"];
            ThreadContext.Properties["Controller"] = filterContext.RouteData.Values["controller"];
            ThreadContext.Properties["IP"] = filterContext.RequestContext.HttpContext.Request.UserHostAddress;

            _errorMessage = filterContext.Exception is HttpRequestValidationException
                                ? "A potentially dangerous request was detected from the client."
                                : filterContext.Exception.Message;

            var userName = "";
            if (HttpContext.Current.User != null)
            {
                userName = HttpContext.Current.User.Identity.Name;
            }

			if (filterContext.Exception is HttpAntiForgeryException) {
				_log.WarnFormat("\nUser name:\n{0}\nException:\n{1}", userName, filterContext.Exception);
			}
			else {
				_log.ErrorFormat("\nUser name:\n{0}\nException:\n{1}", userName, filterContext.Exception);
			}

            if(filterContext.HttpContext.Request.IsAjaxRequest())
            {
                if (filterContext.Controller.GetType()
                                 .GetCustomAttributes(typeof (RestfullErrorHandlingAttribute), true)
                                 .Any())
                {
                    return;
                }
                filterContext.ExceptionHandled = true;
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, error = _errorMessage },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            base.OnException(filterContext);
        }
    }
}
