using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	using System;

	public class Greeting : ABrokerMailToo {
		#region public

		#region constructor

		public Greeting(int customerId, string confirmEmailAddress, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.confirmEmailAddress = confirmEmailAddress;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Greeting"; } } // Name

		#endregion public

		#region protected

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsWhiteLabel) {
				SendToCustomer = false;
			}
		}

		#endregion method LoadRecipientData

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = CustomerData.IsCampaign ? "Greeting - Campaign" : "Greeting";

			Variables = new Dictionary<string, string> {
				{"Email", CustomerData.Mail},
				{"ConfirmEmailAddress", confirmEmailAddress}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region method ActionAtEnd

		protected override void ActionAtEnd() {
			DB.ExecuteNonQuery("Greeting_Mail_Sent",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", CustomerId),
				new QueryParameter("GreetingMailSent", 1),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#endregion protected

		#region private

		private readonly string confirmEmailAddress;

		#endregion private
	} // class Greeting
} // namespace
