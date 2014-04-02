namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	class SpBrokerLoadOwnProperties : AStoredProc
	{
		public SpBrokerLoadOwnProperties(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

		public override bool HasValidParameters()
			if (!string.IsNullOrWhiteSpace(ContactEmail))
				return (BrokerID <= 0) && string.IsNullOrWhiteSpace(ContactMobile);

			if (BrokerID > 0)
				return string.IsNullOrWhiteSpace(ContactMobile);

			return !string.IsNullOrWhiteSpace(ContactMobile);
		} // HasValidParameters

		public string ContactEmail { get; set; }

		public string ContactMobile { get; set; }

		public int BrokerID { get; set; }
	} // SpBrokerLoadOwnProperties

	class SpBrokerLoadOwnProperties2 : AStoredProc
	{
		public SpBrokerLoadOwnProperties2(AConnection oDB, ASafeLog oLog, string mobile) : base(oDB, oLog)
		{
			Mobile = mobile;
		} // constructor

		public override bool HasValidParameters()
		{
			return true;
		} // HasValidParameters

		public string Mobile { get; set; }
	} // SpBrokerLoadOwnProperties2
} // namespace EzBob.Backend.Strategies.Broker
