namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Database;

	public class EmailEnlistedLottery : AMailStrategyBase {
		public EmailEnlistedLottery(int userID, Guid playerID, long lotteryID, bool isBroker) : base(userID, false) {
			Enlisted = false;

			this.isBroker = isBroker;
			this.lotteryID = lotteryID;
			this.doSend = false;
			this.emailAddress = string.Empty;
			this.playerID = playerID;
		} // constructor

		public bool Enlisted { get; private set; }

		public override string Name {
			get { return "EmailEnlistedLottery"; }
		} // Name

		protected override void LoadRecipientData() {
			CustomerData = new CustomerData(this, CustomerId, DB);
			// No need to call to CustomerData.Load() as it is not used in this strategy.

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

			Enlisted = !string.IsNullOrWhiteSpace(this.emailAddress);
			this.doSend = Enlisted && !string.IsNullOrWhiteSpace(TemplateName);
		} // LoadRecipientData

		protected override Addressee[] GetRecipients() {
			return this.doSend ? new [] { new Addressee(this.emailAddress, userID: CustomerId, addSalesforceActivity: !this.isBroker ) } : new Addressee[0];
		} // GetRecipients

		protected override void SetTemplateAndVariables() {
			// Template name is set in LoadRecipinetData().

			Variables = new Dictionary<string, string>(); // Not used in this strategy but should not be empty.
		} // SetTemplateAndVariables

		protected override void SendEmail() {
			if (this.doSend)
				base.SendEmail();
			else
				Log.Debug("Not sending email: enlisted = {0}, template name = '{1}'.", Enlisted, TemplateName);
		} // SendEmail

		protected override void ActionAtEnd() {
			if (!Enlisted) {
				Log.Debug("Not updating player status: not enlisted.");
				return;
			} // if

			// Result is ignored here.
			DB.GetFirst(
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
