namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class SendEmailVerification : AMailStrategyBase {
		public SendEmailVerification(int nCustomerID, string sToken) : base(nCustomerID, true) {
			m_sToken = sToken;
		} // constructor

		public override string Name { get { return "SendEmailVerification"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Confirm your email";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Email", CustomerData.Mail },
				{ "ConfirmEmailAddress", string.Format("<a href='{0}/confirm/{1}'>click here</a>", CustomerData.OriginSite, m_sToken) }
			};
		} // SetTemplateAndVariables

		protected override void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.LoadCustomerOrBroker();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		private readonly string m_sToken;
	} // class SendEmailVerification
} // namespace
