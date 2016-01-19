namespace EzBob.Web.Infrastructure.Filters {
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Code;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public class GlobalAreaAuthorizationFilter : AuthorizeAttribute {
		public enum AreaName {
			Underwriter,
			Customer,
		} // enum AreaName

		public GlobalAreaAuthorizationFilter(RoleCache roleCache, AreaName areaName, string roleName) {
			this.roleCache = roleCache;
			this.areaName = areaName;
			Roles = roleName;
		} // constructor

		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			var routeData = httpContext.Request.RequestContext.RouteData;

			if (!routeData.Values.Any())
				return true;

			string controller = routeData.GetRequiredString("controller");

			if (this.whiteList.Contains(controller))
				return true;

			var area = routeData.DataTokens["area"] as string;

			if (string.IsNullOrEmpty(area))
				return true;

			if (area != this.areaName.ToString())
				return true;

			if (!base.AuthorizeCore(httpContext))
				return false;

			if (!IsStrictArea)
				return true;

			string userName = httpContext.User.Identity.Name;
			CustomerOriginEnum origin = UiCustomerOrigin.Get(httpContext.Request.Url).GetOrigin();

			return 1 == this.roleCache.GetRoleCount(userName, origin);
		} // AuthorizeCore

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
			if (filterContext.HttpContext.Request.IsAjaxRequest()) {
				//use code 423 for ajax request to avoid redirection by forms auth module
				filterContext.Result = new HttpStatusCodeResult(423);
				return;
			} // if

			if (IsAdminPageRedirect) {
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

				if (this.areaName.ToString() == filterContext.RouteData.DataTokens["area"].ToString()) {
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

		private bool IsAdminPageRedirect {
			get { return this.areaName == AreaName.Underwriter; }
		} // IsAdminPageRedirect

		private bool IsStrictArea {
			get { return this.areaName != AreaName.Underwriter; }
		} // IsStrictArea

		private readonly AreaName areaName;

		private readonly string[] whiteList = {
			"Wizard",
			"Start",
			"HowItWorks",
			"AboutUs",
			"WhyEzBob",
			"AmazonMarketPlaces",
		};

		private readonly RoleCache roleCache;
	} // class GlobalAreaAuthorizationFilter
} // namespace
