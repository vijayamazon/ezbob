namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using MailStrategies;

	public class BrokerLeadAcquireCustomer: AStrategy {

		public BrokerLeadAcquireCustomer(
			int nCustomerID,
			int nLeadID,
			string sFirstName,
			bool bBrokerFillsForCustomer,
			string sConfirmationToken
		) {
			m_oSp = new SpBrokerLeadAcquireCustomer(DB, Log) {
				CustomerID = nCustomerID,
				LeadID = nLeadID,
				BrokerFillsForCustomer = bBrokerFillsForCustomer,
			};

			m_sConfirmationToken = sConfirmationToken;
			m_sFirstName = sFirstName;
		} // constructor

		public override string Name {
			get { return "Broker lead acquire customer"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();

			if (!m_oSp.BrokerFillsForCustomer)
				new Greeting(m_oSp.CustomerID, m_sConfirmationToken).Execute();
		} // Execute

		private readonly string m_sConfirmationToken;
		private readonly string m_sFirstName;

		private readonly SpBrokerLeadAcquireCustomer m_oSp;

		private class SpBrokerLeadAcquireCustomer : AStoredProc {
			public SpBrokerLeadAcquireCustomer(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (LeadID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int LeadID { get; set; }

			[UsedImplicitly]
			public bool BrokerFillsForCustomer { get; set; }
		} // SpBrokerLeadAcquireCustomer

	} // class BrokerLeadAcquireCustomer
} // namespace Ezbob.Backend.Strategies.Broker
