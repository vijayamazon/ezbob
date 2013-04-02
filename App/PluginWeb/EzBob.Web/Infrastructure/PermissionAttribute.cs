using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Infrastructure
{
    public class PermissionAttribute : AuthorizeAttribute
    {

        public string Name { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            
            if (string.IsNullOrEmpty(Name)) return;

            var ctx = ObjectFactory.GetInstance<IWorkplaceContext>();

            if (ctx.User.Permissions.All(p => p.Name != Name))
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}