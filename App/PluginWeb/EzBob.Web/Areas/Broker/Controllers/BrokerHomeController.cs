namespace EzBob.Web.Areas.Broker.Controllers {
	using System.Web.Mvc;

	public class BrokerHomeController : Controller {
		#region action Index (default)

		// GET: /Broker/BrokerHome/
		public ActionResult Index() {
			if (User.Identity.IsAuthenticated) {
			} // if

			return View();
		} // Index

		#endregion action Index (default)
	} // class BrokerHomeController
} // namespace
