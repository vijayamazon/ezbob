namespace EzBob.Web.Controllers {
	using System.Web.Mvc;

	public class HeartBeatController : Controller {
		public JsonResult Index() {
			Response.AddHeader("X-FRAME-OPTIONS", "");
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}
	}
}