namespace LegalDocs.Controllers
{
    using System.Web.Mvc;

    public class HomeController : Controller
	{
		//
		// GET: /Home/

		public ActionResult Index()
		{
			if (User.Identity.IsAuthenticated)
			{
                return RedirectToActionPermanent("LegalDocs", "LegalDocs", new { area = "Work" });
			}
			return RedirectToActionPermanent("Login", "Account");
		}

	}
}
