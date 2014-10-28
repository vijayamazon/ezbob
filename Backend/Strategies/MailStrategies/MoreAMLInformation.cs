namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MoreAmlInformation : ABrokerMailToo {
		public MoreAmlInformation(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog)
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

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Application incompleted AML";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class MoreAMLInformation
} // namespace
