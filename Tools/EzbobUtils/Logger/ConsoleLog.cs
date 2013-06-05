using System;

namespace Ezbob.Logger{
	#region class ConsoleLog

	public class ConsoleLog : ASafeLog {
		#region public

		#region constructor

		public ConsoleLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		#endregion constructor

		#region method OwnSay

		public override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

			Console.WriteLine(format, parameters);
		} // OwnSay

		#endregion method OwnSay

		#endregion public
	} // class ConsoleLog

	#endregion class ConsoleLog
} // namespace Ezbob.Logger
