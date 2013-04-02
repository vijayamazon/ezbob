using System;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace EzBob.Web.Infrastructure.csrf
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class,
        AllowMultiple = false, Inherited = true)]
    public class ValidateJsonAntiForgeryTokenAttribute :
        FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            var httpContext = new JsonAntiForgeryHttpContextWrapper(HttpContext.Current);
            AntiForgery.Validate(httpContext, Salt ?? string.Empty);
        }

        public string Salt
        {
            get;
            set;
        }

        // The private context classes go here
    }
}