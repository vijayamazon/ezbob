using ConfigurationBase;
using Ezbob.Logger;

namespace PacnetBalance {
	public class Conf : Configurations {
		public Conf(ASafeLog oLog = null) : base(Consts.GetConfigsSpName, oLog) {
		} // constructor

		public virtual string MailBeeLicenseKey { get; protected set; }
		public virtual string LoginAddress { get; protected set; }
		public virtual string LoginPassword { get; protected set; }
		public virtual string Server { get; protected set; }
		public virtual int Port { get; protected set; }
		public virtual int MailboxReconnectionIntervalSeconds { get; protected set; }
		public virtual int TotalWaitingTime { get; protected set; }
	} // class Conf
} // namespace
