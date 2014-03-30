namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadOwnProperties : AStrategy {
		#region public

		#region constructor

		public BrokerLoadOwnProperties(string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactEmail = sContactEmail,
			};

			Properties = new BrokerProperties();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load own properties"; } // get
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.FillFirst(Properties);
		} // Execute

		#endregion method Execute

		#region property Properties

		public BrokerProperties Properties { get; private set; }

		#endregion property Properties

		#endregion public

		#region private

		private readonly SpBrokerLoadOwnProperties m_oSp;

		#region class SpBrokerLoadOwnProperties

		private class SpBrokerLoadOwnProperties : AStoredProc {
			public SpBrokerLoadOwnProperties(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(ContactEmail);
			} // HasValidParameters

			public string ContactEmail { get; set; }
		} // SpBrokerLoadOwnProperties

		#endregion class SpBrokerLoadOwnProperties

		#endregion private
	} // class BrokerLoadOwnProperties
} // namespace EzBob.Backend.Strategies.Broker
