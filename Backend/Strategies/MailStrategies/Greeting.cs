using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	using System;

	public class Greeting : ABrokerMailToo {

		public Greeting(int customerId, string confirmEmailAddress, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.confirmEmailAddress = confirmEmailAddress;
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
				{"ConfirmEmailAddress", confirmEmailAddress}
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

		private readonly string confirmEmailAddress;

	} // class Greeting
} // namespace
