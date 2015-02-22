namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class EmailChanged : AMailStrategyBase {

		public EmailChanged(
			int nUserID,
			string sRequestId
		)
			: base(nUserID, true) {
				m_sRequestId = sRequestId;
		} // constructor

		public override string Name {
			get { return "Email changed"; }
		} // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Email changed";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Email", CustomerData.Mail },
				{ "Link", string.Format("{0}/emailchanged/{1}", CustomerData.OriginSite, m_sRequestId) },
			};
		} // SetTemplateAndVariables

		protected override void LoadRecipientData() {
			Log.Debug("Loading customer data...");

			CustomerData = new CustomerData(this, CustomerId, DB);
			CustomerData.LoadCustomerOrBroker();

			Log.Debug("Loading customer data complete.");
		} // LoadRecipientData

		private readonly string m_sRequestId;

	} // class EmailChanged
} // namespace
