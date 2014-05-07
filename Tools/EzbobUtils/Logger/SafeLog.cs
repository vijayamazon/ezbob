using System;

namespace Ezbob.Logger {
	using JetBrains.Annotations;

	#region class SafeLog

	public class SafeLog : ASafeLog {
		#region public

		#region constructor

		public SafeLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		#endregion constructor

		#region method OwnSay

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			// nothing here
		} // OwnSay

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			// nothing here
		} // OwnSay

		#endregion method OwnSay

		#endregion public
	} // class SafeLog

	#endregion class SafeLog
} // namespace Ezbob.Logger
