using System.Web.Mvc;

namespace EzBob.Web.Controllers
{
    public class ErrorController : Controller
    {

        public ActionResult NotFound()
        {
            return View();
        }
        public ActionResult Error()
        {
            return View("Error");
        }
    }
}
