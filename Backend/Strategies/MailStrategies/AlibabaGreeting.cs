namespace EzBob.Backend.Strategies.MailStrategies
{
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AlibabaGreeting : ABrokerMailToo
	{
		public AlibabaGreeting(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog)
		{
		}

		public override string Name { get { return "AlibabaGreeting"; } }

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsWhiteLabel) {
				SendToCustomer = false;
			}
		}

		protected override void SetTemplateAndVariables() {
			TemplateName = "Greeting - Alibaba";

			Variables = new Dictionary<string, string> {
				{"Email", CustomerData.Mail}
			};
		}

		protected override void ActionAtEnd() {
			DB.ExecuteNonQuery("Greeting_Mail_Sent",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", CustomerId),
				new QueryParameter("GreetingMailSent", 1),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		}
	}
}
