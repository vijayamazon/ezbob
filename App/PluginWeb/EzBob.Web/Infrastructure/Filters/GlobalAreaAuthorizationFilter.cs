namespace EzBob.Web.Infrastructure.Filters {
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Code;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using StructureMap;

	public class GlobalAreaAuthorizationFilter : AuthorizeAttribute {
		public GlobalAreaAuthorizationFilter(string areaName, string roleName, bool isAdminPageRedirect = false, bool strict = false) {
			Roles = roleName;

			m_sAreaName = areaName;
			m_bIsAdminPageRedirect = isAdminPageRedirect;
			m_bIsStrict = strict;
		} // constructor

		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			var routeData = httpContext.Request.RequestContext.RouteData;

			if (routeData.Values.Any()) {
				var controller = routeData.GetRequiredString("controller");

				if (m_oWhiteList.Contains(controller))
					return true;

				var area = routeData.DataTokens["area"] as string;

				if (string.IsNullOrEmpty(area))
					return true;

				if (area != m_sAreaName)
					return true;

				if (!base.AuthorizeCore(httpContext))
					return false;

				var users = UsersRepository();
				var user = users.GetUserByLogin(httpContext.User.Identity.Name);

				//if strict mode, do not allow to login users that have more than one role
				if (m_bIsStrict && user.Roles.Count > 1)
					return false;
			} // if

			return true;
		} // AuthorizeCore

		protected virtual IUsersRepository UsersRepository() {
			return ObjectFactory.GetInstance<IUsersRepository>();
		} // UsersRepository

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
			if (filterContext.HttpContext.Request.IsAjaxRequest()) {
				//use code 423 for ajax request to avoid rederection by forms auth module
				filterContext.Result = new HttpStatusCodeResult(423);
				return;
			} // if

			if (m_bIsAdminPageRedirect) {
				var workplaceContext = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();

				if (workplaceContext.User != null) {
					var oBrokerHelper = new BrokerHelper();

					if (oBrokerHelper.IsBroker(workplaceContext.User.EMail)) {
						filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
							{"action", "Index"},
							{"controller", "BrokerHome"},
							{"Area", "Broker"}
						});

						return;
					} // if
				} // if

				if (m_sAreaName == filterContext.RouteData.DataTokens["area"].ToString()) {
					filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
						{"action", "AdminLogOn"},
						{"controller", "Account"},
						{"Area", ""},
						{"ReturnUrl", filterContext.HttpContext.Request.RawUrl}
					});

					return;
				} // if
			} // if

			base.HandleUnauthorizedRequest(filterContext);
		} // HandleUnauthorizedRequest

		private readonly string m_sAreaName;
		private readonly bool m_bIsAdminPageRedirect;
		private readonly bool m_bIsStrict;
		private readonly string[] m_oWhiteList = new[] { "Wizard", "Start", "HowItWorks", "AboutUs", "WhyEzBob", "AmazonMarketPlaces" };
	} // class GlobalAreaAuthorizationFilter
} // namespace
