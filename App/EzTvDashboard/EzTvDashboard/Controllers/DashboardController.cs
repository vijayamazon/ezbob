using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EzTvDashboard.Controllers
{
	using Code;

	public class DashboardController : Controller
	{
		//
		// GET: /Dashboard/

		public ActionResult Index()
		{
			var b = new DashboardModelBuilder();
			var model = b.BuildFakeModel();
			return View(model);
		}
	}
}
