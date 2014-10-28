namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class BrokerCheckCustomerRelevance : AStrategy {
		#region public

		#region constructor

		public BrokerCheckCustomerRelevance(
			int nCustomerID,
			string sCustomerEmail,
			bool isAlibaba,
			string sSourceRef,
			string sConfirmEmailLink,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oSp = new SpBrokerCheckCustomerRelevance(DB, Log) {
				CustomerID = nCustomerID,
				CustomerEmail = sCustomerEmail,
				SourceRef = sSourceRef,
			};

			if (isAlibaba)
			{
				m_oGreetingStrat = new AlibabaGreeting(nCustomerID, sConfirmEmailLink, DB, Log);
			}
			else
			{
				m_oGreetingStrat = new Greeting(nCustomerID, sConfirmEmailLink, DB, Log);
			}
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker check customer relevance"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
			m_oGreetingStrat.Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly ABrokerMailToo m_oGreetingStrat;
		private readonly SpBrokerCheckCustomerRelevance m_oSp;

		#region class SpBrokerCheckCustomerRelevance

		private class SpBrokerCheckCustomerRelevance : AStoredProc {
			public SpBrokerCheckCustomerRelevance(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && !string.IsNullOrWhiteSpace(CustomerEmail);
			} // HasValidParameters

			public int CustomerID { get; set; }

			public string CustomerEmail { get; set; }

			public string SourceRef { get; set; }
		} // class SpBrokerCheckCustomerRelevance

		#endregion class SpBrokerCheckCustomerRelevance

		#endregion private
	} // class BrokerCheckCustomerRelevance
} // namespace EzBob.Backend.Strategies.Broker
