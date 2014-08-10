namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EmailChanged : AMailStrategyBase {
		#region public

		#region constructor

		public EmailChanged(
			int nUserID,
			string sAddress,
			AConnection oDB,
			ASafeLog oLog
		) : base(nUserID, true, oDB, oLog) {
			m_sAddress = sAddress;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Email changed"; }
		} // Name

		#endregion property Name

		#endregion public

		#region protected

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Email changed";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Email", CustomerData.Mail },
				{ "Link", m_sAddress }
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

		#endregion protected

		#region private

		private readonly string m_sAddress;

		#endregion private
	} // class EmailChanged
} // namespace
