namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EmailChanged : AMailStrategyBase {

		public EmailChanged(
			int nUserID,
			string sAddress,
			AConnection oDB,
			ASafeLog oLog
		) : base(nUserID, true, oDB, oLog) {
			m_sAddress = sAddress;
		} // constructor

		public override string Name {
			get { return "Email changed"; }
		} // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Email changed";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Email", CustomerData.Mail },
				{ "Link", m_sAddress }
			};
		} // SetTemplateAndVariables

		protected override void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.LoadCustomerOrBroker();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		private readonly string m_sAddress;

	} // class EmailChanged
} // namespace
