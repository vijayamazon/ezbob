namespace EzBob.Web.Infrastructure {
	using System.Collections.Generic;
	using StructureMap;
	using System.Web.Mvc;
	using System.Linq;

	public class PermissionAttribute : AuthorizeAttribute {
		public string Name { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			if (string.IsNullOrEmpty(Name))
				return;

			if (ObjectFactory.GetInstance<IWorkplaceContext>().UserPermissions.All(p => p.Name != Name))
				filterContext.Result = new HttpUnauthorizedResult();
		} // OnAuthorization
	} // class PermissionAttribute

	public class PermissionsAttribute : AuthorizeAttribute {
		public string[] Names { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext) {
			base.OnAuthorization(filterContext);

			if (Names == null || !Names.Any())
				return;

			if (!ObjectFactory.GetInstance<IWorkplaceContext>().UserPermissions.Select(x => x.Name).Intersect(Names).Any())
				filterContext.Result = new HttpUnauthorizedResult();
		} // OnAuthorization
	} // class PermissionAttribute
} // namespace