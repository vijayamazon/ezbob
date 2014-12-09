namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class MoreBwaInformation : ABrokerMailToo {
		public MoreBwaInformation(int customerId) : base(customerId, true) {
		} // constructor

		public override string Name { get { return "MoreBWAInformation"; } } // Name

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker)
				SendToCustomer = false;
		} // LoadRecipientData

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Application incompleted Bank";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

	} // class MoreBWAInformation
} // namespace
