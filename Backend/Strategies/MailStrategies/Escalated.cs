using System;
using System.Data;
using System.Globalization;
using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class Escalated : AMailStrategyBase {
		#region constructor

		public Escalated(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, false, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Escalated"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - User was escalated";

			DataTable dt = DB.ExecuteReader(
				"GetEscalationData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId)
			);

			if (dt.Rows.Count < 1)
				throw new StrategyException(this, "failed to load escalation data from customer " + CustomerData);

			var sr = new SafeReader(dt.Rows[0]);

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

		#endregion method SetTemplateAndVariables
	} // class Escalated
} // namespace
