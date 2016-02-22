namespace LegalDocs.Code.Filters {
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public class PermissionAttribute : AuthorizeAttribute {
        public string Name { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext) {
            base.OnAuthorization(filterContext);

            if (string.IsNullOrEmpty(Name))
                return;

            string userName = HttpContext.Current.User.Identity.Name;

            var user = Session.Instance.GetUser(userName, 1);

            if (user == null) {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            var premissions = Name.Split(Convert.ToChar(",")).ToArray();

            foreach (var role in user.UserRoles) {
                if (role.Premissions != null) {
                    var intersectRoles = role.Premissions.Select(x => x.Name).Intersect(premissions).ToList();
                    if (intersectRoles.Any()) {
                        return;
                    }
                }
            }
            filterContext.Result = new HttpUnauthorizedResult();

        } // OnAuthorization
    } // class PermissionAttribute
} // namespace