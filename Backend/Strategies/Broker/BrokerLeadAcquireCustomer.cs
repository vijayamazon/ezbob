namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class BrokerLeadAcquireCustomer: AStrategy {
		#region public

		#region constructor

		public BrokerLeadAcquireCustomer(int nCustomerID, int nLeadID, string sEmailConfirmationLink, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerLeadAcquireCustomer(DB, Log) {
				CustomerID = nCustomerID,
				LeadID = nLeadID,
				BrokerFillsForCustomer = string.IsNullOrWhiteSpace(sEmailConfirmationLink),
			};

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
			m_oSp.ExecuteNonQuery();

			if (!string.IsNullOrWhiteSpace(m_sEmailConfirmationLink))
				new Greeting(m_oSp.CustomerID, m_sEmailConfirmationLink, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly string m_sEmailConfirmationLink;

		private readonly SpBrokerLeadAcquireCustomer m_oSp;

		#region class SpBrokerLeadAcquireCustomer

		private class SpBrokerLeadAcquireCustomer : AStoredProc {
			public SpBrokerLeadAcquireCustomer(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (LeadID > 0);
			} // HasValidParameters

			public int CustomerID { get; set; }
			public int LeadID { get; set; }

			public bool BrokerFillsForCustomer { get; set; }
		} // SpBrokerLeadAcquireCustomer

		#endregion class SpBrokerLeadAcquireCustomer

		#endregion private
	} // class BrokerLeadAcquireCustomer
} // namespace EzBob.Backend.Strategies.Broker
