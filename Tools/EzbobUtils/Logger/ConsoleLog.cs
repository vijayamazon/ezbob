using System;

namespace Ezbob.Logger{
	#region class ConsoleLog

	public class ConsoleLog : ASafeLog {
		#region public

		#region constructor

		public ConsoleLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method OwnSay

		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

			Console.WriteLine(format, parameters);
		} // OwnSay

		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

			Console.WriteLine(format, parameters);

			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());
			Console.WriteLine(ExceptionToString(ex));
		} // OwnSay

		#endregion method OwnSay

		#endregion protected
	} // class ConsoleLog

	#endregion class ConsoleLog
} // namespace Ezbob.Logger
