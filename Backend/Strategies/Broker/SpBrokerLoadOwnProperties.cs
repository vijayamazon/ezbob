namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	class SpBrokerLoadOwnProperties : AStoredProc {
		public SpBrokerLoadOwnProperties(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

		public override bool HasValidParameters() {
			if (!string.IsNullOrWhiteSpace(ContactEmail))
				return (BrokerID <= 0) && string.IsNullOrWhiteSpace(ContactMobile) && (Origin > 0);

			if (BrokerID > 0)
				return string.IsNullOrWhiteSpace(ContactMobile);

			return !string.IsNullOrWhiteSpace(ContactMobile);
		} // HasValidParameters

		public string ContactEmail { get; set; }

		public string ContactMobile { get; set; }

		public int BrokerID { get; set; }

		public int Origin { get; set; }
	} // SpBrokerLoadOwnProperties
} // namespace Ezbob.Backend.Strategies.Broker
