namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System.Web.Mvc;
	using Infrastructure;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Infrastructure.Attributes;

	public class InvestorController : Controller {
		private readonly IEzbobWorkplaceContext context;
		private readonly IUsersRepository users;

		public InvestorController(
			IEzbobWorkplaceContext context,
			IUsersRepository users) {
			this.context = context;
			this.users = users;
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonResult Index() {
			return Json(new { });
		}
	}
}