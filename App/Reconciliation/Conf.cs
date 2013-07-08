using ConfigurationBase;

namespace Reconciliation {
	public class Conf : ConfigurationsBase {
		static Conf() {
			Init(typeof(Conf), Consts.GetConfigsSpName);
		} // static constructor

		public static string MailBeeLicenseKey { get; private set; }
		public static string LoginAddress { get; private set; }
		public static string LoginPassword { get; private set; }
		public static string Server { get; private set; }
		public static int Port { get; private set; }
		public static int MailboxReconnectionIntervalSeconds { get; private set; }

		public static string PayPointMid { get; private set; }
		public static string PayPointVpnPassword { get; private set; }
		public static string PayPointRemotePassword { get; private set; }
	} // class Conf
} // namespace
