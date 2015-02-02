namespace EzBob.Web.Code {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class ScratchHelper {
		public ScratchHelper(int userID, string playerID) {
			this.serviceClient = new ServiceClient();
			this.userID = userID;
			this.lotteryPlayerID = ParsePlayerID(playerID);
		} // constructor

		public LotteryResult PlayLottery() {
			if (this.lotteryPlayerID == null)
				return new LotteryResult();

			try {
				LotteryActionResult lar = serviceClient.Instance.PlayLottery(
					this.userID,
					this.lotteryPlayerID.Value
				);

				return lar.Value;
			} catch (Exception e) {
				log.Alert(e, "Failed to play lottery with player id '{0}'.", this.lotteryPlayerID);
				return new LotteryResult();
			} // try
		} // PlayLottery

		public void Claim() {
			if (this.lotteryPlayerID == null)
				return;

			try {
				serviceClient.Instance.ChangeLotteryPlayerStatus(
					this.userID,
					this.lotteryPlayerID.Value,
					LotteryPlayerStatus.Played
				);
			} catch (Exception e) {
				log.Alert(e, "Failed to claim user lottery result with player id '{0}'.", this.lotteryPlayerID);
			} // try
		} // Claim

		public void Decline() {
			if (this.lotteryPlayerID == null)
				return;

			try {
				serviceClient.Instance.ChangeLotteryPlayerStatus(
					this.userID,
					this.lotteryPlayerID.Value,
					LotteryPlayerStatus.Excluded
				);
			} catch (Exception e) {
				log.Alert(e, "Failed to decline participation in lottery with player id '{0}'.", this.lotteryPlayerID);
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

		private static readonly ASafeLog log = new SafeILog(typeof(ScratchHelper));

		private readonly ServiceClient serviceClient;
		private readonly int userID;
		private readonly Guid? lotteryPlayerID;
	} // class ScratchHelper
} // namespace