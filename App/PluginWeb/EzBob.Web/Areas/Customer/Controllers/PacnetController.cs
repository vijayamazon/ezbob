using System.Web.Mvc;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class PacnetController : Controller
    {
        public ActionResult Error()
        {
            return View("PacnetError");
        }
    }
}