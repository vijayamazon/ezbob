namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Lottery;
	using Ezbob.Backend.Strategies.MailStrategies;

	partial class EzServiceImplementation {
		public ActionMetaData ChangeLotteryPlayerStatus(int customerID, Guid playerID, LotteryPlayerStatus newStatus) {
			return Execute<ChangeLotteryPlayerStatus>(customerID, null, playerID, newStatus);
		} // ChangeLotteryPlayerStatus

		public LotteryActionResult PlayLottery(int customerID, Guid playerID) {
			PlayLottery instance;

			ActionMetaData amd = ExecuteSync(out instance, customerID, null, playerID);

			return new LotteryActionResult {
				Value = instance.Result,
				MetaData = amd,
			};
		} // PlayLottery

		public ActionMetaData EnlistLottery(int customerID) {
			return Execute<EnlistLottery>(customerID, null, customerID, DB, Log);
		} // EnlistLottery
	} // class EzServiceImplementation
} // namespace
