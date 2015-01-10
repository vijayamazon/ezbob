namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class ChangeLotteryPlayerStatus : AStrategy {
		public override string Name {
			get { return "ChangeLotteryPlayerStatus"; }
		} // Name

		public ChangeLotteryPlayerStatus(Guid playerID, LotteryPlayerStatus newStatus) {
			this.playerID = playerID;
			this.newStatus = newStatus;
		} // constructor

		public override void Execute() {
			DB.ExecuteNonQuery(
				"UpdatePlayerStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("PlayerID", this.playerID),
				new QueryParameter("StatusID", (long)(int)this.newStatus),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // Execute

		private readonly Guid playerID;
		private readonly LotteryPlayerStatus newStatus;
	} // class ChangeLotteryPlayerStatus
} // namespace

