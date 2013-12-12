namespace EzBob.Backend.Strategies.MailStrategies
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Collections.Generic;
	using DbConnection;

	public class Escalated : MailStrategyBase
	{
		public Escalated(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "User was Escalated";
			TemplateName = "Mandrill - User was escalated";

			DataTable dt = DbConnection.ExecuteSpReader("GetEscalationData", DbConnection.CreateParam("CustomerId", CustomerId));
			DataRow results = dt.Rows[0];

			string escalationReason = results["EscalationReason"].ToString();
			string underwriterName = results["UnderwriterName"].ToString();
			DateTime registrationDate = DateTime.Parse(results["GreetingMailSentDate"].ToString());
			string medal = results["MedalType"].ToString();
			string systemDecision = results["SystemDecision"].ToString();

			Variables = new Dictionary<string, string>
				{
					{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", CustomerData.Mail},
					{"EscalationReason", escalationReason},
					{"UWName", underwriterName},
					{"RegistrationDate", registrationDate.ToString(CultureInfo.InvariantCulture)},
					{"FirstName", CustomerData.FirstName},
					{"Surname", CustomerData.Surname},
					{"MedalType", medal},
					{"SystemDecision", systemDecision}
				};
		}
	}
}
