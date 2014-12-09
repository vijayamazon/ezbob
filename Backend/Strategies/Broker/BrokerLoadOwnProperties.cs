namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Backend.Models;

	public class BrokerLoadOwnProperties : AStrategy {

		public BrokerLoadOwnProperties(string sContactEmail, int nBrokerID) {
			m_oSp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactEmail = sContactEmail,
				BrokerID = nBrokerID,
			};

			Properties = new BrokerProperties();
		} // constructor

		public override string Name {
			get { return "Broker load own properties"; } // get
		} // Name

		public override void Execute() {
			m_oSp.FillFirst(Properties);
		} // Execute

		public BrokerProperties Properties { get; private set; }

		private readonly SpBrokerLoadOwnProperties m_oSp;

	} // class BrokerLoadOwnProperties
} // namespace Ezbob.Backend.Strategies.Broker
