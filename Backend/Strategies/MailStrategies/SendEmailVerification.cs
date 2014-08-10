namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SendEmailVerification : AMailStrategyBase {
		#region constructor

		public SendEmailVerification(
			int nCustomerID,
			string sAddress,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, true, oDB, oLog) {
			m_sAddress = sAddress;
		} // constructor

		#endregion constructor

		public override string Name { get { return "SendEmailVerification"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Confirm your email";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Email", CustomerData.Mail },
				{ "ConfirmEmailAddress", m_sAddress }
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.LoadCustomerOrBroker();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		private readonly string m_sAddress;
	} // class SendEmailVerification
} // namespace
