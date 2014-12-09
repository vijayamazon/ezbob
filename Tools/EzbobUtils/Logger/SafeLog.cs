using System;

namespace Ezbob.Logger {
	using JetBrains.Annotations;

	public class SafeLog : ASafeLog {

		public SafeLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			// nothing here
		} // OwnSay

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			// nothing here
		} // OwnSay

	} // class SafeLog

} // namespace Ezbob.Logger
