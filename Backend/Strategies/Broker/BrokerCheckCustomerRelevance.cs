﻿namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using MailStrategies;

	public class BrokerCheckCustomerRelevance : AStrategy {
		public BrokerCheckCustomerRelevance(
			int nCustomerID,
			string sCustomerEmail,
			bool isAlibaba,
			string sSourceRef,
			string sConfirmationToken
		) {
			m_oSp = new SpBrokerCheckCustomerRelevance(DB, Log) {
				CustomerID = nCustomerID,
				CustomerEmail = sCustomerEmail,
				SourceRef = sSourceRef,
			};

			if (isAlibaba)
				m_oGreetingStrat = new AlibabaGreeting(nCustomerID, sConfirmationToken);
			else
				m_oGreetingStrat = new Greeting(nCustomerID, sConfirmationToken);
		} // constructor

		public override string Name {
			get { return "Broker check customer relevance"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
			m_oGreetingStrat.Execute();
		} // Execute

		private readonly ABrokerMailToo m_oGreetingStrat;
		private readonly SpBrokerCheckCustomerRelevance m_oSp;

		private class SpBrokerCheckCustomerRelevance : AStoredProc {
			public SpBrokerCheckCustomerRelevance(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && !string.IsNullOrWhiteSpace(CustomerEmail);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public string CustomerEmail { get; set; }

			[UsedImplicitly]
			public string SourceRef { get; set; }
		} // class SpBrokerCheckCustomerRelevance

	} // class BrokerCheckCustomerRelevance
} // namespace Ezbob.Backend.Strategies.Broker
