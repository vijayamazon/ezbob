namespace EzBob.Web.Infrastructure {
	using StructureMap;
	using System.Web.Mvc;
	using System.Linq;

	public class PermissionAttribute : AuthorizeAttribute {
		public string Name { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			if (string.IsNullOrEmpty(Name))
				return;

			if (!filterContext.HttpContext.User.Identity.IsAuthenticated) {
				var urlHelper = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
				filterContext.Result = new RedirectResult(urlHelper.Action("AdminLogon", "Account", new {
					Area = "",
					ReturnUrl = filterContext.HttpContext.Server.UrlEncode(filterContext.HttpContext.Request.RawUrl)
				}));
					
				return;
			} // if

			var context = ObjectFactory.GetInstance<IWorkplaceContext>();

			if (context.UserPermissions.All(p => p.Name != Name))
				filterContext.Result = new HttpStatusCodeResult(423);
		} // OnAuthorization
	} // class PermissionAttribute

	public class PermissionsAttribute : AuthorizeAttribute {
		public string[] Names { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			if (Names == null || !Names.Any())
				return;

			if (!ObjectFactory.GetInstance<IWorkplaceContext>().UserPermissions.Select(x => x.Name).Intersect(Names).Any())
				filterContext.Result = new HttpStatusCodeResult(423);
		} // OnAuthorization
	} // class PermissionAttribute
} // namespace