using System.Web.Mvc;

namespace EzTvDashboard.Controllers
{
	using Code;
	using Models;

	public class DashboardController : Controller
	{
		//
		// GET: /Dashboard/
		static DashboardModel _model;
		
		public ActionResult Index()
		{
			var b = new DashboardModelBuilder();
			if (b.SomethingChanged() || _model == null)
			{
				_model = b.BuildFakeModel();
			}
			Response.AddHeader("Refresh", "2");
			return View(_model);
		}
	}
}
