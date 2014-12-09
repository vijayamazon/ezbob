namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerSaveCrmEntry : AStrategy {

		public BrokerSaveCrmEntry(
			string sType,
			int nActionID,
			int nStatusID,
			string sComment,
			string sCustomerRefNum,
			string sContactEmail,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_sType = sType;
			m_nActionID = nActionID;
			m_nStatusID = nStatusID;
			m_sComment = (sComment ?? string.Empty).Trim();
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = (sContactEmail ?? string.Empty).Trim();
			ErrorMsg = null;

			if (m_sComment.Length > 1000)
				m_sComment = m_sComment.Substring(0, 1000);
		} // constructor

		public virtual string ErrorMsg { get; private set; }

		public override string Name {
			get { return "Broker: save CRM entry"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail)) {
				ErrorMsg = "Broker contact email not specified.";
				return;
			} // if

			if (string.IsNullOrWhiteSpace(m_sComment)) {
				ErrorMsg = "CRM comment not specified.";
				return;
			} // if

			try {
				ErrorMsg = DB.ExecuteScalar<string>(
					"BrokerSaveCrmEntry",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@Type", m_sType),
					new QueryParameter("@ActionID", m_nActionID),
					new QueryParameter("@StatusID", m_nStatusID),
					new QueryParameter("@Comment", m_sComment),
					new QueryParameter("@RefNum", m_sCustomerRefNum),
					new QueryParameter("@ContactEmail", m_sContactEmail),
					new QueryParameter("@EntryTime", DateTime.UtcNow)
				);
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to save CRM entry.");
				ErrorMsg = "Failed to save CRM entry.";
			} // try
		} // Execute

		private readonly int m_nActionID;
		private readonly int m_nStatusID;
		private readonly string m_sComment;
		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly string m_sType;

	} // class BrokerSaveCrmEntry

} // namespace EzBob.Backend.Strategies.Broker
