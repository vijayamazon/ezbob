namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	class SpBrokerLoadOwnProperties : AStoredProc
	{
		public SpBrokerLoadOwnProperties(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

		public override bool HasValidParameters()
		{
			return !string.IsNullOrWhiteSpace(ContactEmail) || (BrokerID > 0);
		} // HasValidParameters

		public string ContactEmail { get; set; }

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
