using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EzTvDashboard.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/

		public ActionResult Index()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToActionPermanent("Index", "Dashboard");
			}
			return RedirectToActionPermanent("Login", "Account");
		}

	}
}
