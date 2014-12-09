namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class BrokerBackFromCustomerWizard : AStrategy {
		public BrokerBackFromCustomerWizard(int nLeadID) {
			m_oSp = new BrokerLeadLoadBroker(DB, Log) {
				LeadID = nLeadID,
			};

			m_oResultRow = null;
		} // constructor

		public override string Name {
			get { return "Broker back from customer wizard"; } // get
		} // Name

		public string ContactEmail {
			get { return (m_oResultRow == null) ? string.Empty : m_oResultRow.BrokerContactEmail; } // get
		} // ContactEmail

		public override void Execute() {
			m_oResultRow = m_oSp.FillFirst<BrokerLeadLoadBroker.ResultRow>();
		} // Execute

		private readonly BrokerLeadLoadBroker m_oSp;
		private BrokerLeadLoadBroker.ResultRow m_oResultRow;

		private class BrokerLeadLoadBroker : AStoredProcedure {
			public BrokerLeadLoadBroker(AConnection oDB, ASafeLog oLog = null) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return LeadID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int LeadID { get; set; } // LeadID

			public class ResultRow : AResultRow {
				[UsedImplicitly]
				public int BrokerID { get; set; } // BrokerID
				[UsedImplicitly]
				public string BrokerContactEmail { get; set; } // BrokerContactEmail
				[UsedImplicitly]
				public int CustomerID { get; set; } // CustomerID
				[UsedImplicitly]
				public bool IsAtLastWizardStep { get; set; } // IsAtLastWizardStep
			} // class ResultRow
		} // BrokerLeadLoadBroker

	} // class BrokerBackFromCustomerWizard
} // namespace Ezbob.Backend.Strategies.Broker
