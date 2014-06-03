using System.Web.Mvc;

namespace EzTvDashboard.Controllers
{
	using Code;
	using Models;

	[Authorize]
	public class DashboardController : Controller
	{
		//
		// GET: /Dashboard/
		static DashboardModel _model;
		private DashboardModelBuilder _modelBuilder;

		public DashboardController()
		{
			_modelBuilder = new DashboardModelBuilder();
		}

		public ActionResult Index()
		{
			_model = _modelBuilder.BuildModel();
			
			//Response.AddHeader("Refresh", "2");
			return View(_model);
		}

		
		[HttpGet]
		public JsonResult IsSomethingChanged()
		{
			return Json(new { changed = _modelBuilder.SomethingChanged(), lastChanged = _model.LastChanged }, JsonRequestBehavior.AllowGet);
		}
	}
}
