namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using EzBob.Web.Infrastructure;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class Ny2015ScratchController : Controller {
		public Ny2015ScratchController(IEzbobWorkplaceContext context) {
			this.serviceClient = new ServiceClient();
			this.context = context;
		} // constructor

		[HttpGet]
		public JsonResult PlayLottery(string playerID) {
			Guid? lotteryPlayerID = ParsePlayerID(playerID);

			if (lotteryPlayerID == null)
				return Json(new LotteryResult(), JsonRequestBehavior.AllowGet);

			try {
				LotteryActionResult lar = serviceClient.Instance.PlayLottery(
					DetectCustomer(),
					lotteryPlayerID.Value
				);

				return Json(lar.Value, JsonRequestBehavior.AllowGet);
			} catch (Exception e) {
				log.Alert(e, "Failed to play lottery with player id '{0}'.", lotteryPlayerID);
				return Json(new LotteryResult(), JsonRequestBehavior.AllowGet);
			} // try
		} // PlayLottery

		[HttpPost]
		public void Claim(string playerID) {
			Guid? lotteryPlayerID = ParsePlayerID(playerID);

			if (lotteryPlayerID == null)
				return;

			try {
				serviceClient.Instance.ChangeLotteryPlayerStatus(
					DetectCustomer(),
					lotteryPlayerID.Value,
					LotteryPlayerStatus.Played
				);
			} catch (Exception e) {
				log.Alert(e, "Failed to claim user lottery result with player id '{0}'.", lotteryPlayerID);
			} // try
		} // Claim

		[HttpPost]
		public void Decline(string playerID) {
			Guid? lotteryPlayerID = ParsePlayerID(playerID);

			if (lotteryPlayerID == null)
				return;

			try {
				serviceClient.Instance.ChangeLotteryPlayerStatus(
					DetectCustomer(),
					lotteryPlayerID.Value,
					LotteryPlayerStatus.Excluded
				);
			} catch (Exception e) {
				log.Alert(e, "Failed to decline participation in lottery with player id '{0}'.", lotteryPlayerID);
			} // try
		} // Decline

		private static Guid? ParsePlayerID(string playerID) {
			try {
				return new Guid(playerID);
			} catch (Exception e) {
				log.Alert(e, "Failed to parse player id from '{0}'.", playerID);
				return null;
			} // try
		} // ParsePlayerID

		private int DetectCustomer() {
			try {
				return this.context.Customer.Id;
			}
			catch (Exception e) {
				log.Warn(e, "Failed to fetch current customer.");
				return 0;
			} // try
		} // DetectCustomer

		private static readonly ASafeLog log = new SafeILog(typeof(Ny2015ScratchController));

		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;
	} // class Ny2015ScratchController
} // namespace
