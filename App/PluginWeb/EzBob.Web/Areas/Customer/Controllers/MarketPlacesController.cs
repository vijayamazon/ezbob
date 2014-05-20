namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Web.Mvc;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;

	public class MarketPlacesController : Controller {
		#region public

		#region constructor

		public MarketPlacesController(IEzbobWorkplaceContext context) {
			_context = context;
		} // constructor

		#endregion constructor

		#region method Accounts (account list by type)

		[ValidateJsonAntiForgeryToken]
		[HttpGet, Ajax]
		public JsonResult Accounts() {

			return Json(new { mpAccounts = _context.Customer.GetMarketPlaces() }, JsonRequestBehavior.AllowGet);
		} // Accounts

		#endregion method Accounts (account list by type)
		
		#endregion public

		#region private

		#region fields

		private readonly IEzbobWorkplaceContext _context;

		#endregion fields

		#endregion private
	} // class MarketPlacesController
} // namespace