namespace Ezbob.Backend.Strategies.MailStrategies {
	using ConfigManager;
	using MailApi;

	public class CustomerBankAccountIsAlreadyAddedEmail : AStrategy {

		public CustomerBankAccountIsAlreadyAddedEmail(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name { get { return "Customer Bank Account Is Already Added Email"; } }// Name
		public override void Execute() {
			if (string.IsNullOrEmpty(CurrentValues.Instance.FraudAlertMailReciever)) {
				this.Log.Warn("FraudAlertMailReciever is null or empty, not sending alert");
				return;
			}
			string message = string.Format("Bank account provided for funds transfer for loan of customer {0} was found identical to bank account used for another loan under different account", this.customerID);
			new Mail().Send(
					CurrentValues.Instance.FraudAlertMailReciever,
					message,
					null,
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					string.Format("Fraud Alert - {0} provided with identical bank account as other customer", this.customerID)
				);
		}

		private readonly int customerID;
	} // class CustomerBankAccountIsAlreadyAddedEmail
} // namespace
