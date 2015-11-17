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
			string message = string.Format(@"Bank account provided for funds transfer for loan of customer {0} was found identical to bank account used for another loan under different account. 

Please note that the only NONE FRAUD scenario is of two companies under same ownership & same bank account but with two separate loan requests.

Funds transfer is currently stopped.

Please review the case and resume the funds transfer only if you are sure this is not Fraud attempt.

In case you are investigating the issue - you can set the Fraud status to ""Under Investigation"" and leave it this way until further decision will be taken.

In case you are confident with resuming the funds transfer, please do the following:
1. Set Block taking loan to ""No"".
2. Contact the client and request to proceed with the agreement confirmation process.

Thank you.", this.customerID);
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
