namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	class SpBrokerLoadOwnProperties : AStoredProc {
		public SpBrokerLoadOwnProperties(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

		public override bool HasValidParameters() {
			return !string.IsNullOrWhiteSpace(ContactEmail) || (BrokerID > 0);
		} // HasValidParameters

		public string ContactEmail { get; set; }

		public int BrokerID { get; set; }
	} // SpBrokerLoadOwnProperties
} // namespace EzBob.Backend.Strategies.Broker
