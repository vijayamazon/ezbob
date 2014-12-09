namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Exceptions;

	public class Escalated : AMailStrategyBase {

		public Escalated(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, false, oDb, oLog) {
		} // constructor

		public override string Name { get { return "Escalated"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - User was escalated";

			SafeReader sr = DB.GetFirst(
				"GetEscalationData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId)
			);

			if (sr.IsEmpty)
				throw new StrategyWarning(this, "failed to load escalation data from customer " + CustomerData);

			string escalationReason = sr["EscalationReason"];
			string underwriterName = sr["UnderwriterName"];
			DateTime registrationDate = sr["GreetingMailSentDate"];
			string medal = sr["MedalType"];
			string systemDecision = sr["SystemDecision"];

			Variables = new Dictionary<string, string> {
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
		} //  SetTemplateAndVariables

	} // class Escalated
} // namespace
