using ConfigurationBase;
using Ezbob.Logger;

namespace PayPointBalance {
	public class Conf : Configurations {
		public Conf(ASafeLog oLog = null) : base("GetPacnetAgentConfigs", oLog) {
		} // constructor

		public virtual string PayPointMid { get; protected set; }
		public virtual string PayPointVpnPassword { get; protected set; }
		public virtual string PayPointRemotePassword { get; protected set; }
		public virtual int PayPointRetryCount { get; protected set; }
		public virtual int PayPointSleepInterval { get; protected set; }
	} // class Conf
} // namespace
