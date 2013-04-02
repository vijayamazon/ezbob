using System.Linq;
using System.Web.Mvc;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class HelperController : Controller
    {
        private readonly IWorkplaceContext _context;

        public HelperController(IWorkplaceContext context)
        {
            _context = context;
        }

        [ChildActionOnly]
        public PartialViewResult PermissionClasses()
        {
            var permissions = _context.User.Permissions.Select(p => p.Name).ToArray();
            return PartialView(permissions);
        }
        [ChildActionOnly]
        public PartialViewResult RolesClasses()
        {
            var roles = _context.User.Roles.Select(p => p.Name).ToArray();
            return PartialView(roles);
        }
    }
}