namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SendEmailVerification : AMailStrategyBase {
		#region constructor

		public SendEmailVerification(
			int nCustomerID,
			string sFirstName,
			string sEmail,
			string sAddress,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, true, oDB, oLog) {
			m_sFirstName = sFirstName;
			m_sAddress = sAddress;
			m_sEmail = sEmail;
		} // constructor

		#endregion constructor

		public override string Name { get { return "SendEmailVerification"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Confirm your email";

			Variables = new Dictionary<string, string> {
				{ "FirstName", m_sFirstName },
				{ "Email", m_sEmail },
				{ "ConfirmEmailAddress", m_sAddress }
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			// Nothing here.
		} // LoadRecipientData

		#endregion method LoadRecipientData

		private readonly string m_sAddress;
		private readonly string m_sEmail;
		private readonly string m_sFirstName;
	} // class SendEmailVerification
} // namespace
