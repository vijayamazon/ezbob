namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
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
			SafeReader sr = DB.GetFirst(
				"UpdatePlayerStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("PlayerID", this.playerID),
				new QueryParameter("StatusID", (long)(int)this.newStatus),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			if (newStatus != LotteryPlayerStatus.Played)
				return;

			if (sr.IsEmpty)
				return;

			int userID = sr["UserID"];

			if (userID < 1)
				return;

			string mailTemplateName = sr["WinMailTemplateName"];

			if (string.IsNullOrWhiteSpace(mailTemplateName))
				return;

			decimal amount = sr["Amount"];

			if (amount <= 0)
				return;

			new SendWinInLotteryMail(userID, mailTemplateName, amount, sr["Description"]).Execute();
		} // Execute

		private readonly Guid playerID;
		private readonly LotteryPlayerStatus newStatus;
	} // class ChangeLotteryPlayerStatus
} // namespace

