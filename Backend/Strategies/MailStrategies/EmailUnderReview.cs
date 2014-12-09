namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EmailUnderReview : ABrokerMailToo {

		public EmailUnderReview(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {} // constructor

		public override string Name { get { return "Email Under Review"; } }

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker)
				SendToCustomer = false;
		} // LoadRecipientData

		protected override void SetTemplateAndVariables() {
			TemplateName = CustomerData.IsAlibaba ? "Mandrill - Alibaba application completed under review" : "Mandrill - Application completed under review";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

	} // class EmailUnderReview
} // namespace
