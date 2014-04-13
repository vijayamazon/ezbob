namespace EzBob.Web.Infrastructure.Filters
{
	using System;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Models;
	using NHibernate;
	using StructureMap;
	using ConfigManager;

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class WhiteListFilter : FilterAttribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!CurrentValues.Instance.LandingPageEnabled) return;

            HandleWhiteListRequest(filterContext);
        }

        protected virtual void HandleWhiteListRequest(AuthorizationContext filterContext)
        {

            var session = ObjectFactory.GetInstance<ISession>();

            if (filterContext.RouteData.Values["controller"].ToString() == "Landing")
            {
                return;
            }

            var emailCookie = filterContext.HttpContext.Request.Cookies["Email"];

            if (emailCookie != null)
            {

                var email = emailCookie.Value;
                if (session.QueryOver<AllowedEmail>().Where(c => c.Email == email).RowCount() > 0)
                {
                    return;
                }
            }

            // only redirect for GET requests, otherwise the browser might not propagate the verb and request
            // body correctly. 

            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Can redirect only get requests");
            }

            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                        {
                            Action= "Index", 
                            Controller = "Landing",
                            Area = "",
                            ReturnUrl = filterContext.HttpContext.Request.RawUrl
                        }));
        }

    }
}