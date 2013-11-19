namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Web.Mvc;
	using Infrastructure;
	using log4net;
	using Scorto.Web;
	using Models;

	public class MarketPlacesController : Controller {
		#region public

		#region constructor

		public MarketPlacesController(IEzbobWorkplaceContext context) {
			_context = context;
		} // constructor

		#endregion constructor

		#region method Accounts (account list by type)

		[Transactional, HttpGet, Ajax]
		public JsonNetResult Accounts() {

			return this.JsonNet(new { mpAccounts = _context.Customer.GetMarketPlaces() });
		} // Accounts

		#endregion method Accounts (account list by type)
		
		#endregion public

		#region private

		#region fields

		private readonly IEzbobWorkplaceContext _context;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CGMarketPlacesController));

		#endregion fields

		#endregion private
	} // class MarketPlacesController
} // namespace