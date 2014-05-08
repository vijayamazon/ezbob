namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using Infrastructure.Attributes;
	using Models;
	using System.Web.Mvc;

	public class PropertiesController : Controller
	{
		public PropertiesController()
		{
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var data = new PropertiesModel(11,2,1000,400);
			return Json(data, JsonRequestBehavior.AllowGet);
		}
	}
}
