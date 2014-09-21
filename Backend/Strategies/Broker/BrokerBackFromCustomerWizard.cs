namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerBackFromCustomerWizard : AStrategy {
		#region public

		#region constructor

		public BrokerBackFromCustomerWizard(int nLeadID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new BrokerLeadLoadBroker(DB, Log) {
				LeadID = nLeadID,
			};

			m_oResultRow = null;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker back from customer wizard"; } // get
		} // Name

		#endregion property Name

		#region property ContactEmail

		public string ContactEmail {
			get { return (m_oResultRow == null) ? string.Empty : m_oResultRow.BrokerContactEmail; } // get
		} // ContactEmail

		#endregion property ContactEmail

		#region method Execute

		public override void Execute() {
			m_oResultRow = m_oSp.FillFirst<BrokerLeadLoadBroker.ResultRow>();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly BrokerLeadLoadBroker m_oSp;
		private BrokerLeadLoadBroker.ResultRow m_oResultRow;

		#region class BrokerLeadLoadBroker

		private class BrokerLeadLoadBroker : AStoredProcedure {
			public BrokerLeadLoadBroker(AConnection oDB, ASafeLog oLog = null) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return LeadID > 0;
			} // HasValidParameters

			public int LeadID { get; set; } // LeadID

			public class ResultRow : AResultRow {
				public int BrokerID { get; set; } // BrokerID
				public string BrokerContactEmail { get; set; } // BrokerContactEmail
				public int CustomerID { get; set; } // CustomerID
				public bool IsAtLastWizardStep { get; set; } // IsAtLastWizardStep
			} // class ResultRow
		} // BrokerLeadLoadBroker

		#endregion class BrokerLeadLoadBroker

		#endregion private
	} // class BrokerBackFromCustomerWizard
} // namespace EzBob.Backend.Strategies.Broker
