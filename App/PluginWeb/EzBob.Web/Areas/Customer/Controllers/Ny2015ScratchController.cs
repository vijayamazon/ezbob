namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Web.Mvc;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;

	public class Ny2015ScratchController : Controller {
		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PlayLottery(string playerID, int userID) {
			var ny2015ScratchHelper = new Ny2015ScratchHelper(userID, playerID);
			return Json(ny2015ScratchHelper.PlayLottery(), JsonRequestBehavior.AllowGet);
		} // PlayLottery

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public void Claim(string playerID, int userID) {
			var ny2015ScratchHelper = new Ny2015ScratchHelper(userID, playerID);
			ny2015ScratchHelper.Claim();
		} // Claim

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public void Decline(string playerID, int userID) {
			var ny2015ScratchHelper = new Ny2015ScratchHelper(userID, playerID);
			ny2015ScratchHelper.Decline();
		} // Decline
	} // class Ny2015ScratchController
} // namespace
