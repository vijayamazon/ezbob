namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System.Collections.Generic;
	using System.Web.Mvc;
	using EzBob.Web.Areas.Underwriter.Models.Investor;
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
		public JsonResult Index() {
			return Json(new { });
		}

		[Ajax]
		[HttpPost]
		public JsonResult AddInvestor(InvestorModel investor, InvestorContactModel investorContact, List<InvestorBankAccountModel> InvestorBank, bool SameBank) {
			//todo save to db
			return Json(new {
				investor,
				investorContact,
				InvestorBank,
				SameBank
			});
		}
	}
}