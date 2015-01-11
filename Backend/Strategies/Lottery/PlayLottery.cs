namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Backend.Strategies;

	public class PlayLottery : AStrategy {
		public override string Name {
			get { return "PlayLottery"; }
		} // Name

		public PlayLottery(Guid playerID, AConnection db, ASafeLog log) : base(db, log) {
			this.playerID = playerID;
			Result = new LotteryResult();
		} // constructor

		public override void Execute() {
			DB.FillFirst(
				Result,
				"PlayLottery",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LotteryPlayerID", this.playerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // Execute

		public LotteryResult Result { get; private set; }

		private readonly Guid playerID;
	} // class PlayLottery
} // namespace

