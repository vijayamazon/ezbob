using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ApplicationMng.Repository;
using StructureMap;

namespace EzBob.Web.Infrastructure.Filters
{
    public class GlobalAreaAuthorizationFilter : AuthorizeAttribute
    {
        private readonly string _areaName;
        private readonly bool _isAdminPageRedirect;
        private readonly bool _strict;
        private readonly string[] _whiteList = new[] { "Wizard", "Start", "HowItWorks", "AboutUs", "WhyEzBob", "AmazonMarketPlaces" };
        //--------------------------------------------------------------------------
        public GlobalAreaAuthorizationFilter(string areaName, string roleName, bool isAdminPageRedirect = false, bool strict = false)
        {
            _areaName = areaName;
            Roles = roleName;
            _isAdminPageRedirect = isAdminPageRedirect;
            _strict = strict;
            Roles = roleName;
        }
        //---------------------------------------------------------------------------
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var routeData = httpContext.Request.RequestContext.RouteData;

            var controller = routeData.GetRequiredString("controller");

            if (_whiteList.Contains(controller)) return true;

            var area = routeData.DataTokens["area"] as string;

            if(string.IsNullOrEmpty(area)) return true;

            if (area != _areaName) return true;

            if (!base.AuthorizeCore(httpContext)) return false;

            var users = UsersRepository();
            var user = users.GetUserByLogin(httpContext.User.Identity.Name);

            //if strict mode, do not allow to login users that have more than one role
            if (_strict && user.Roles.Count > 1) return false;

            return true;
        }

        protected virtual IUsersRepository UsersRepository()
        {
            var users = ObjectFactory.GetInstance<IUsersRepository>();
            return users;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (_isAdminPageRedirect && _areaName == filterContext.RouteData.DataTokens["area"].ToString())
            {
                var redirectRouteDict = new RouteValueDictionary
                    {
                        {"action", "AdminLogOn"},
                        {"controller", "Account"},
                        {"Area", ""},
                        {"ReturnUrl", filterContext.HttpContext.Request.RawUrl}
                    };
                filterContext.Result = new RedirectToRouteResult(redirectRouteDict);
            } else
            {
                base.HandleUnauthorizedRequest(filterContext);                
            }
        }
    }
}