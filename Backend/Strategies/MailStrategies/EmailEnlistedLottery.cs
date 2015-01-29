namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Database;

	public class EmailEnlistedLottery : AMailStrategyBase {
		public EmailEnlistedLottery(int userID, Guid playerID, long lotteryID, bool isBroker) : base(userID, false) {
			this.isBroker = isBroker;
			this.lotteryID = lotteryID;
			this.doSend = false;
			this.emailAddress = string.Empty;
			this.playerID = playerID;
		} // constructor

		public override string Name {
			get { return "EmailEnlistedLottery"; }
		} // Name

		protected override void LoadRecipientData() {
			SafeReader sr = DB.GetFirst(
				"EnlistLottery",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", CustomerId),
				new QueryParameter("UniqueID", this.playerID),
				new QueryParameter("LotteryID", this.lotteryID),
				new QueryParameter("IsBroker", this.isBroker),
				new QueryParameter("Now", DateTime.Now)
			);

			if (!sr.IsEmpty) {
				this.emailAddress = sr["Email"];

				TemplateName = sr["TemplateName"];

				Variables = new Dictionary<string, string> {
					{ "ContactName", sr["ContactName"] },
				};
			} // if

			this.doSend = !string.IsNullOrWhiteSpace(this.emailAddress);
		} // LoadRecipientData

		protected override Addressee[] GetRecipients() {
			return this.doSend ? new [] { new Addressee(this.emailAddress) } : new Addressee[0];
		} // GetRecipients

		protected override void SetTemplateAndVariables() {
			// Template name and variables are set in LoadRecipinetData().
		} // SetTemplateAndVariables

		protected override void SendEmail() {
			if (this.doSend)
				base.SendEmail();
		} // SendEmail

		protected override void ActionAtEnd() {
			if (!this.doSend)
				return;

			DB.ExecuteNonQuery(
				"UpdatePlayerStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("PlayerID", this.playerID),
				new QueryParameter("StatusID", (long)(int)LotteryPlayerStatus.NotPlayed),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // ActionAtEnd

		private readonly Guid playerID;
		private bool doSend;
		private string emailAddress;
		private readonly long lotteryID;
		private readonly bool isBroker;
	} // class EmailEnlistedLottery
} // namespace
