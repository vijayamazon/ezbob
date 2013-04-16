using System.Web.Mvc;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class FraudController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

    }
}
