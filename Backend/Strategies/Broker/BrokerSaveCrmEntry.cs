namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerSaveCrmEntry

	public class BrokerSaveCrmEntry : AStrategy {
		#region public

		#region constructor

		public BrokerSaveCrmEntry(
			bool bIsIncoming,
			int nActionID,
			int nStatusID,
			string sComment,
			int nCustomerID,
			string sContactEmail,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_bIsIncoming = bIsIncoming;
			m_nActionID = nActionID;
			m_nStatusID = nStatusID;
			m_sComment = (sComment ?? string.Empty).Trim();
			m_nCustomerID = nCustomerID;
			m_sContactEmail = (sContactEmail ?? string.Empty).Trim();
			ErrorMsg = null;

			if (m_sComment.Length > 1000)
				m_sComment = m_sComment.Substring(0, 1000);
		} // constructor

		#endregion constructor

		#region property ErrorMsg

		public virtual string ErrorMsg { get; private set; }

		#endregion property ErrorMsg

		#region property Name

		public override string Name {
			get { return "Broker: save CRM entry"; }
		} // Name

		#endregion property Name

		#region method Execute

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
					new QueryParameter("@IsIncoming", m_bIsIncoming),
					new QueryParameter("@ActionID", m_nActionID),
					new QueryParameter("@StatusID", m_nStatusID),
					new QueryParameter("@Comment", m_sComment),
					new QueryParameter("@CustomerID", m_nCustomerID),
					new QueryParameter("@ContactEmail", m_sContactEmail),
					new QueryParameter("@EntryTime", DateTime.UtcNow)
				);
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to save CRM entry.");
				ErrorMsg = "Failed to save CRM entry.";
			} // try
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private bool m_bIsIncoming;
		private int m_nActionID;
		private int m_nStatusID;
		private string m_sComment;
		private int m_nCustomerID;
		private string m_sContactEmail;

		#endregion private
	} // class BrokerSaveCrmEntry

	#endregion class BrokerSaveCrmEntry
} // namespace EzBob.Backend.Strategies.Broker
