using System;

namespace Ezbob.Logger{

	public class ConsoleLog : ASafeLog {

		public ConsoleLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		public override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

			Console.WriteLine(format, parameters);
		} // OwnSay

	} // class ConsoleLog

} // namespace Ezbob.Logger
