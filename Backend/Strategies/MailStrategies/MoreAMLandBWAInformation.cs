namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MoreAmlAndBwaInformation : ABrokerMailToo {
		public MoreAmlAndBwaInformation(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog)
		{
		} // constructor

		public override string Name { get { return "More AML and BWA Information"; } } // Name

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker)
				SendToCustomer = false;
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Application incompleted AML & Bank";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class MoreAMLandBWAInformation
} // namespace
