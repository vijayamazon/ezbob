namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class SendWinInLotteryMail : AMailStrategyBase {
		public SendWinInLotteryMail(
			int userID,
			string mailTemplateName,
			decimal amount,
			string prizeDescription
		) : base(userID, true) {
			this.mailTemplateName = mailTemplateName;
			this.amount = amount;
			this.prizeDescription = prizeDescription;
		} // constructor

		public override string Name {
			get { return "SendWinInLotteryMail"; }
		} // Name

		protected override void LoadRecipientData() {
			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.LoadCustomerOrBroker();
		} // LoadRecipientData

		protected override void SetTemplateAndVariables() {
			TemplateName = this.mailTemplateName;

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Amount", this.amount.ToString("C2", Library.Instance.Culture) },
				{ "PrizeDescription", this.prizeDescription },
			};
		} // SetTemplateAndVariables

		private readonly string mailTemplateName;
		private readonly decimal amount;
		private readonly string prizeDescription;
	} // class SendWinInLotteryMail
} // namespace
