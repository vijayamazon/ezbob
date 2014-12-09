namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Web.Mvc;
	using EzBob.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;

	public class MarketPlacesController : Controller {

		public MarketPlacesController(IEzbobWorkplaceContext context) {
			_context = context;
		} // constructor

		[ValidateJsonAntiForgeryToken]
		[HttpGet, Ajax]
		public JsonResult Accounts() {

			return Json(new { mpAccounts = _context.Customer.GetMarketPlaces() }, JsonRequestBehavior.AllowGet);
		} // Accounts

		private readonly IEzbobWorkplaceContext _context;

	} // class MarketPlacesController
} // namespace
