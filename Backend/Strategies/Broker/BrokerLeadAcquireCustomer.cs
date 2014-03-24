namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class BrokerLeadAcquireCustomer: AStrategy {
		#region public

		#region constructor

		public BrokerLeadAcquireCustomer(int nCustomerID, int nLeadID, string sEmailConfirmationLink, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_nLeadID = nLeadID;
			m_sEmailConfirmationLink = sEmailConfirmationLink;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker lead acquire customer"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if ((m_nCustomerID < 1) || (m_nLeadID < 1))
				return;

			DB.ExecuteNonQuery(
				"BrokerLeadAcquireCustomer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@LeadID", m_nLeadID)
			);

			if (!string.IsNullOrWhiteSpace(m_sEmailConfirmationLink))
				new Greeting(m_nCustomerID, m_sEmailConfirmationLink, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly int m_nLeadID;
		private readonly string m_sEmailConfirmationLink;

		#endregion private
	} // class BrokerLeadAcquireCustomer
} // namespace EzBob.Backend.Strategies.Broker
