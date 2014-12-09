namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	public class AlibabaGreeting : ABrokerMailToo {
		private readonly string confirmEmailAddress;

		public AlibabaGreeting(int customerId, string confirmEmailAddress)
			: base(customerId, true) {
			this.confirmEmailAddress = confirmEmailAddress;
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
				{"Email", CustomerData.Mail},
				{"FirstName", CustomerData.FirstName},
				{"ConfirmEmailAddress", confirmEmailAddress}
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
