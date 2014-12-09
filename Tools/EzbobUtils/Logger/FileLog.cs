using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ezbob.Logger {
	using JetBrains.Annotations;

	public class FileLog : ASafeLog {

		public FileLog(
			string sAppName = null,
			bool bAppend = false,
			bool bUtcDateInName = false,
			bool bUtcTimeInName = false,
			string sPath = null,
			ASafeLog oLog = null
		) : base(oLog)
		{
			AppName = (sAppName ?? string.Empty).Trim();

			if (AppName == string.Empty)
				AppName = Assembly.GetCallingAssembly().GetName().Name;

			sPath = (sPath ?? string.Empty).Trim();

			var oFileName = new StringBuilder(AppName);

			if (bUtcTimeInName)
				oFileName.Append(DateTime.UtcNow.ToString(".yyyy-MM-dd.HH-mm-ss", CultureInfo.InvariantCulture));
			else if (bUtcDateInName)
				oFileName.Append(DateTime.UtcNow.ToString(".yyyy-MM-dd", CultureInfo.InvariantCulture));

			oFileName.Append(".log");

			string sFileName = (sPath != string.Empty) ? Path.Combine(sPath, oFileName.ToString()) : oFileName.ToString();

			try {
				m_oLogFile = new StreamWriter(sFileName, bAppend, Encoding.UTF8);
			}
			catch (Exception e) {
				throw new LogException("Failed to open a log file " + sFileName, e);
			} // try

			m_oLock = new object();

			NotifyStartStop("started");
			SayCurrentTimezone();
		} // constructor

		public override void Dispose() {
			if (m_oLogFile != null) {
				NotifyStartStop("stopped");
				m_oLogFile.Close();
			} // if

			m_oLogFile = null;

			base.Dispose();
		} // Dispose

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			lock (m_oLock) {
				m_oLogFile.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

				m_oLogFile.WriteLine(format, parameters);

				m_oLogFile.Flush();
			} // lock
		} // OwnSay

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			lock (m_oLock) {
				m_oLogFile.Write("{0} {1} ", CurrentTime, nSeverity.ToString());

				m_oLogFile.WriteLine(format, parameters);

				m_oLogFile.Flush();

				m_oLogFile.Write("{0} {1} ", CurrentTime, nSeverity.ToString());
				m_oLogFile.WriteLine(ExceptionToString(ex));

				m_oLogFile.Flush();
			} // lock
		} // OwnSay

		protected virtual string AppName { get; private set; }

		protected virtual void NotifyStartStop(string sEvent) {
			Msg(
				"Logging {0} for {1} v{5} on {2} as {3} with pid {4}",
				sEvent,
				AppName,
				System.Environment.MachineName,
				System.Environment.UserName,
				Process.GetCurrentProcess().Id,
				Assembly.GetCallingAssembly().GetName().Version.ToString(4)
			);
		} // NotifyStartStop

		private StreamWriter m_oLogFile;
		private readonly object m_oLock;

	} // class FileLog

} // namespace Ezbob.Logger
