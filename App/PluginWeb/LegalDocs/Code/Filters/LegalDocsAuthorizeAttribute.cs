namespace LegalDocs.Code.Filters {
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public class LegalDocsAuthorizeAttribute : AuthorizeAttribute {

        protected override bool AuthorizeCore(HttpContextBase httpContext) {

            if (this.whiteList.Contains(httpContext.Request.RequestContext.RouteData.GetRequiredString("controller")))
                return true;

            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            if (string.IsNullOrEmpty(Roles)) {
                return true;
            }


            return true;
        }

        private readonly string[] whiteList = {
            "Account",
            "Home"
		};

    }
}