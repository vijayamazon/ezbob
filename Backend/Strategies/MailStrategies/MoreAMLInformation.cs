namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class MoreAmlInformation : ABrokerMailToo {
		public MoreAmlInformation(int customerId)
			: base(customerId, true)
		{
		} // constructor

		public override string Name { get { return "More AML Information"; } } // Name

		protected override void LoadRecipientData()
		{
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker)
			{
				SendToCustomer = false;
			}
		}

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Application incompleted AML";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

	} // class MoreAMLInformation
} // namespace
