namespace EzAutoResponder
{
	using ConfigurationBase;
	using Ezbob.Logger;

	class Conf: Configurations {
		public Conf(ASafeLog oLog = null) : base(Const.GetConfigsSpName, oLog) {
		} // constructor

		public virtual string MailBeeLicenseKey { get; protected set; }
		//public virtual string LoginAddress { get; protected set; }
		//public virtual string LoginPassword { get; protected set; }
		public virtual string Server { get; protected set; }
		public virtual int Port { get; protected set; }
		public virtual int MailboxReconnectionIntervalSeconds { get; protected set; }
		public virtual int TotalWaitingTime { get; protected set; }

		public virtual string TestAddress { get; protected set; }
		public virtual string TestPassword { get; protected set; }
	} // class Conf
	
}
