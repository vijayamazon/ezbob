using ConfigurationBase;

namespace PayPointBalance {
	public class Conf : ConfigurationsBase {
		static Conf() {
			Init(typeof(Conf), "GetPacnetAgentConfigs");
		} // static constructor

		public static string PayPointMid { get; private set; }
		public static string PayPointVpnPassword { get; private set; }
		public static string PayPointRemotePassword { get; private set; }
		public static int PayPointRetryCount { get; private set; }
		public static int PayPointSleepInterval { get; private set; }
	} // class Conf
} // namespace
