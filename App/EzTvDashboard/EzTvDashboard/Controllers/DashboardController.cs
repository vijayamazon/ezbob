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
		static DashboardModel _model;
		private readonly DashboardModelBuilder _modelBuilder;

		public DashboardController()
		{
			_modelBuilder = new DashboardModelBuilder();
		}

		[HttpGet]
		public ActionResult Index()
		{
			_model = _modelBuilder.GetModel();
			return View(_model);
		}

		[HttpGet]
		public ActionResult Dashboard()
		{
			_model = _modelBuilder.GetModel();
			return PartialView(_model);
		}

		[HttpGet]
		public ActionResult Redirect()
		{
			return RedirectToAction("Index");
		}
		
		[HttpGet]
		public JsonResult IsSomethingChanged()
		{
			return Json(new { changed = _modelBuilder.SomethingChanged(), lastChanged = _model.LastChanged }, JsonRequestBehavior.AllowGet);
		}
	}
}
