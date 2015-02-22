namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	public class AlibabaGreeting : ABrokerMailToo {
		private readonly string confirmationToken;

		public AlibabaGreeting(int customerId, string confirmationToken)
			: base(customerId, true) {
				this.confirmationToken = confirmationToken;
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
				{"ConfirmEmailAddress", string.Format("<a href='{0}/confirm/{1}'>click here</a>", CustomerData.OriginSite, confirmationToken)}
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
