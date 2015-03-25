namespace Ezbob.Logger{
	using System;
	using JetBrains.Annotations;

	public class ConsoleLog : ASafeLog {
		public ConsoleLog(ASafeLog oLog = null) : base(oLog) {} // constructor

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

			Console.WriteLine(format, parameters);
		} // OwnSay

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

			Console.WriteLine(format, parameters);

			Console.Write("{0} {1} ", CurrentTime, nSeverity.ToString());
			Console.WriteLine(ExceptionToString(ex));
		} // OwnSay
	} // class ConsoleLog
} // namespace Ezbob.Logger
