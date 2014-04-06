namespace EzBob.Web.Controllers {
	using System.Web.Mvc;

	public class HeartBeatController : Controller {
		public JsonResult Index() {
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}
	}
}