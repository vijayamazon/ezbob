namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class MoreAmlAndBwaInformation : ABrokerMailToo {
		public MoreAmlAndBwaInformation(int customerId)
			: base(customerId, true) {
		} // constructor

		public override string Name { get { return "More AML and BWA Information"; } } // Name

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker)
				SendToCustomer = false;
		} // LoadRecipientData

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Application incompleted AML & Bank";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

	} // class MoreAMLandBWAInformation
} // namespace
