namespace Ezbob.Logger {

	public class SafeLog : ASafeLog {

		public SafeLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		public override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			// nothing here
		} // OwnSay

	} // class SafeLog

} // namespace Ezbob.Logger
