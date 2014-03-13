namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Backend.Strategies.MailStrategies;

	public class BrokerAddCustomerLead : AStrategy {
		#region public

		#region constructor

		public BrokerAddCustomerLead(string sLeadFirstName, string sLeadLastName, string sLeadEmail, string sLeadAddMode, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sLeadFirstName = sLeadFirstName;
			m_sLeadLastName = sLeadLastName;
			m_sLeadEmail = sLeadEmail;
			m_sLeadAddMode = sLeadAddMode;
			m_sContactEmail = (sContactEmail ?? string.Empty).Trim();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker: add customer lead"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (
				string.IsNullOrWhiteSpace(m_sLeadFirstName) ||
				string.IsNullOrWhiteSpace(m_sLeadLastName) ||
				string.IsNullOrWhiteSpace(m_sLeadEmail) ||
				string.IsNullOrWhiteSpace(m_sLeadAddMode) ||
				string.IsNullOrWhiteSpace(m_sContactEmail)
			) {
				return;
			} // if

			string sErrorMsg = null;
			int nLeadID = 0;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					sErrorMsg = sr["ErrorMsg"];
					nLeadID = sr["LeadID"];
					return ActionResult.SkipAll;
				},
				"BrokerAddCustomerLead",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LeadFirstName", m_sLeadFirstName),
				new QueryParameter("@LeadLastName", m_sLeadLastName),
				new QueryParameter("@LeadEmail", m_sLeadEmail),
				new QueryParameter("@LeadAddMode", m_sLeadAddMode),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@DateCreated", DateTime.UtcNow)
			);

			if (!string.IsNullOrWhiteSpace(sErrorMsg))
				throw new Exception(sErrorMsg);

			if (nLeadID < 1)
				throw new Exception("Failed to add a customer lead.");

			new BrokerLeadSendInvitation(nLeadID, m_sContactEmail, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly string m_sLeadFirstName;
		private readonly string m_sLeadLastName;
		private readonly string m_sLeadEmail;
		private readonly string m_sLeadAddMode;
		private readonly string m_sContactEmail;

		#endregion private
	} // class BrokerAddCustomerLead
} // namespace EzBob.Backend.Strategies.Broker
