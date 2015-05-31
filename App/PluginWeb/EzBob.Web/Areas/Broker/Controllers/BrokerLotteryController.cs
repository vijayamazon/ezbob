namespace EzBob.Web.Areas.Broker.Controllers {
	using System.Web.Mvc;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;

	public class BrokerLotteryController : ABrokerBaseController {
		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PlayLottery(string playerID, int userID) {
			var helper = new ScratchHelper(userID, playerID);
			return Json(helper.PlayLottery(), JsonRequestBehavior.AllowGet);
		} // PlayLottery

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public void Claim(string playerID, int userID) {
			var helper = new ScratchHelper(userID, playerID);
			helper.Claim();
		} // Claim

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public void Decline(string playerID, int userID) {
			var helper = new ScratchHelper(userID, playerID);
			helper.Decline();
		} // Decline
	} // class BrokerLotteryController
} // namespace
