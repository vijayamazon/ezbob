namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	public class Greeting : ABrokerMailToo {

		public Greeting(int customerId, string confirmationToken) : base(customerId, true) {
			this.confirmationToken = confirmationToken;
		} // constructor

		public override string Name { get { return "Greeting"; } } // Name

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsWhiteLabel) {
				SendToCustomer = false;
			}
		}

		protected override void SetTemplateAndVariables() {
			TemplateName = CustomerData.IsCampaign ? "Greeting - Campaign" : "Greeting";

			Variables = new Dictionary<string, string> {
				{"Email", CustomerData.Mail},
				{"ConfirmEmailAddress", string.Format("<a href='{0}/confirm/{1}'>click here</a>", CustomerData.OriginSite, confirmationToken)}
			};
		} // SetTemplateAndVariables

		protected override void ActionAtEnd() {
			DB.ExecuteNonQuery("Greeting_Mail_Sent",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", CustomerId),
				new QueryParameter("GreetingMailSent", 1),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // ActionAtEnd

		private readonly string confirmationToken;

	} // class Greeting
} // namespace
