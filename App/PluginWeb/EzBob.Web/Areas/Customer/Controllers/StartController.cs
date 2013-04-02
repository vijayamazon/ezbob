using System.Web.Mvc;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class StartController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            return RedirectToAction("Index", "Profile", new {Area = "Customer"});
            ViewData["returnUrl"] = returnUrl;
            return View("_Start");
        }

        public ActionResult TopButton()
        {
            return PartialView("~/Views/Shared/_TopButtons.cshtml");
        }

        //------------------------------------------
        public ActionResult MyProfile()
        {
            return PartialView("~/Views/Shared/_MyProfile.cshtml");
        }
    }
}
