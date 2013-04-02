using System.Web.Mvc;

namespace EzBob.Web.Controllers
{
    public class PieController : Controller
    {
        public FileResult Index()
        {
            return File(Server.MapPath("~/Content/pie.htc"), "text/x-component");
        }
    }
}
