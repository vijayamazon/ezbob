namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GreetingForBrokerFilled : ABrokerMailToo {
		#region public

		#region constructor

		public GreetingForBrokerFilled(
			int nCustomerID,
			string sFirstName,
			string sConfirmEmailLink,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, true, oDB, oLog) {
			m_sConfirmEmailLink = sConfirmEmailLink;
			m_sFirstName = sFirstName;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Greeting for Broker filled"; } } // Name

		#endregion public

		#region protected

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Greeting for broker filled";

			Variables = new Dictionary<string, string> {
				{ "Email", CustomerData.Mail },
				{ "FirstName", string.IsNullOrWhiteSpace(m_sFirstName) ? string.Empty : m_sFirstName.Trim() },
				{ "ConfirmEmailLink", m_sConfirmEmailLink },
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

		private readonly string m_sConfirmEmailLink;
		private readonly string m_sFirstName;

		#endregion private
	} // class GreetingForBrokerFilled
} // namespace
