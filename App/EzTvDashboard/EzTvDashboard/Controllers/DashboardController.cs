namespace EzTvDashboard.Controllers
{
	using System.Web.Mvc;
	using Code;
	using Models;

	[Authorize]
	public class DashboardController : Controller
	{
		//
		// GET: /Dashboard/
		private readonly DashboardModelBuilder _modelBuilder;

		public DashboardController()
		{
			_modelBuilder = new DashboardModelBuilder();
		}

		[HttpGet]
		public ActionResult Index()
		{
			var model = _modelBuilder.GetModel();
			return View(model);
		}

		[HttpGet]
		public ActionResult Dashboard()
		{
			var model = _modelBuilder.GetModel();
			return PartialView(model);
		}

		[HttpGet]
		public ActionResult Redirect()
		{
			return RedirectToAction("Index");
		}
		
		[HttpGet]
		public JsonResult IsSomethingChanged()
		{
			var changed = _modelBuilder.SomethingChanged();
			return Json(new { changed = changed, lastChanged = DashboardModelBuilder.LastChanged }, JsonRequestBehavior.AllowGet);
		}
	}
}
