namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class SendEmailVerification : AMailStrategyBase {
		public SendEmailVerification(int nCustomerID, string sAddress) : base(nCustomerID, true) {
			m_sAddress = sAddress;
		} // constructor

		public override string Name { get { return "SendEmailVerification"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Confirm your email";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Email", CustomerData.Mail },
				{ "ConfirmEmailAddress", m_sAddress }
			};
		} // SetTemplateAndVariables

		protected override void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.LoadCustomerOrBroker();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		private readonly string m_sAddress;
	} // class SendEmailVerification
} // namespace
