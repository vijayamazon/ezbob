namespace Ezbob.Logger {
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Text;
	using JetBrains.Annotations;

	#region enum LoggingEvent

	public enum LoggingEvent {
		Started,
		Stopped,
	} // enum LoggingEvent

	#endregion enum LoggingEvent

	#region class ASafeLog

	public abstract class ASafeLog : IDisposable {
		#region public

		#region method SetInternal

		public virtual void SetInternal(ASafeLog oLog, bool bOverwriteExisting = false) {
			lock (ms_oLock) {
				if (m_oLog == null)
					m_oLog = oLog;
				else if (bOverwriteExisting)
					m_oLog = oLog;
			} // if
		} // SetInternal

		#endregion method SetInternal

		#region method NotifyStartStop

		public virtual void NotifyStart(Severity nSeverity = Severity.Info) {
			NotifyStartStop(LoggingEvent.Started, Assembly.GetCallingAssembly().GetName(), nSeverity);
		} // NotifyStart

		public virtual void NotifyStop(Severity nSeverity = Severity.Info) {
			NotifyStartStop(LoggingEvent.Stopped, Assembly.GetCallingAssembly().GetName(), nSeverity);
		} // NotifyStart

		protected virtual void NotifyStartStop(LoggingEvent nEvent, AssemblyName oCallingAssemblyName, Severity nSeverity = Severity.Info) {
			AssemblyName oName = oCallingAssemblyName ?? Assembly.GetCallingAssembly().GetName();

			Say(
				nSeverity,
				"Logging {0} for {1} v{5} on {2} as {3} with pid {4}.",
				nEvent.ToString().ToLower(),
				oName.FullName,
				System.Environment.MachineName,
				System.Environment.UserName,
				Process.GetCurrentProcess().Id,
				oName.Version.ToString(4)
			);
		} // NotifyStartStop

		#endregion method NotifyStartStop

		#region method Say

		public virtual void Say(Severity nSeverity, object obj) { Say(nSeverity, (obj ?? "<null>").ToString()); } // Say
		public virtual void Say(Severity nSeverity, string message) { Say(nSeverity, "{0}", message ?? "<null>"); } // Say

		[StringFormatMethod("format")]
		public virtual void Say(Severity nSeverity, string format, params object[] parameters) {
			if (format == null)
				OwnSay(nSeverity, "<null>");
			else
				OwnSay(nSeverity, format, parameters);

			if (m_oLog != null)
				m_oLog.Say(nSeverity, format, parameters);
		} // Say

		[StringFormatMethod("format")]
		public virtual void Say(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			if (format == null)
				OwnSay(nSeverity, ex, "<null>");
			else
				OwnSay(nSeverity, ex, format, parameters);

			if (m_oLog != null)
				m_oLog.Say(nSeverity, ex, format, parameters);
		} // Say

		#endregion method Say

		#region method Debug

		public virtual void Debug(object obj) { Debug((obj ?? "<null>").ToString()); } // Debug
		[StringFormatMethod("format")]
		public virtual void Debug(string format, params object[] parameters) { Say(Severity.Debug, format, parameters); } // Debug
		[StringFormatMethod("format")]
		public virtual void Debug(Exception ex, string format, params object[] parameters) { Say(Severity.Debug, ex, format, parameters); } // Debug

		#endregion method Debug

		#region method Msg

		public virtual void Msg(object obj) { Msg((obj ?? "<null>").ToString()); } // Msg
		[StringFormatMethod("format")]
		public virtual void Msg(string format, params object[] parameters) { Say(Severity.Msg, format, parameters); } // Msg
		[StringFormatMethod("format")]
		public virtual void Msg(Exception ex, string format, params object[] parameters) { Say(Severity.Msg, ex, format, parameters); } // Msg

		#endregion method Msg

		#region method Info

		public virtual void Info(object obj) { Info((obj ?? "<null>").ToString()); } // Info
		[StringFormatMethod("format")]
		public virtual void Info(string format, params object[] parameters) { Say(Severity.Info, format, parameters); } // Info
		[StringFormatMethod("format")]
		public virtual void Info(Exception ex, string format, params object[] parameters) { Say(Severity.Info, ex, format, parameters); } // Info

		#endregion method Info

		#region method Warn

		public virtual void Warn(object obj) { Warn((obj ?? "<null>").ToString()); } // Warn
		[StringFormatMethod("format")]
		public virtual void Warn(string format, params object[] parameters) { Say(Severity.Warn, format, parameters); } // Warn
		[StringFormatMethod("format")]
		public virtual void Warn(Exception ex, string format, params object[] parameters) { Say(Severity.Warn, ex, format, parameters); } // Warn

		#endregion method Warn

		#region method Error

		public virtual void Error(object obj) { Error((obj ?? "<null>").ToString()); } // Error
		[StringFormatMethod("format")]
		public virtual void Error(string format, params object[] parameters) { Say(Severity.Error, format, parameters); } // Error
		[StringFormatMethod("format")]
		public virtual void Error(Exception ex, string format, params object[] parameters) { Say(Severity.Error, ex, format, parameters); } // Error

		#endregion method Error

		#region method Alert

		public virtual void Alert(object obj) { Alert((obj ?? "<null>").ToString()); } // Alert
		[StringFormatMethod("format")]
		public virtual void Alert(string format, params object[] parameters) { Say(Severity.Alert, format, parameters); } // Alert
		[StringFormatMethod("format")]
		public virtual void Alert(Exception ex, string format, params object[] parameters) { Say(Severity.Alert, ex, format, parameters); } // Alert

		#endregion method Alert

		#region method Fatal

		public virtual void Fatal(object obj) { Fatal((obj ?? "<null>").ToString()); } // Fatal
		[StringFormatMethod("format")]
		public virtual void Fatal(string format, params object[] parameters) { Say(Severity.Fatal, format, parameters); } // Fatal
		[StringFormatMethod("format")]
		public virtual void Fatal(Exception ex, string format, params object[] parameters) { Say(Severity.Fatal, ex, format, parameters); } // Fatal

		#endregion method Fatal

		#region IDisposable implementation

		#region method Dispose

		public virtual void Dispose() {
			// nothing here
		} // Dispose

		#endregion method Dispose

		#endregion IDisposable implementation

		#endregion public

		#region protected

		#region constructor

		protected ASafeLog(ASafeLog oLog = null) {
			m_oLog = oLog;
		} // constructor

		#endregion constructor

		#region property CurrentTime

		protected virtual string CurrentTime {
			get { return DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.ffffff", CultureInfo.InvariantCulture); }
		} // CurrentTime

		#endregion property CurrentTime

		#region method SayCurrentTimezone

		protected virtual void SayCurrentTimezone() {
			Msg("<- this is UTC time.");
		} // SayCurrentTimezone

		#endregion method SayCurrentTimezone

		#region method ExceptionToString

		protected virtual string ExceptionToString(Exception ex) {
			if (ex == null)
				return "<null>";

			var os = new StringBuilder();

			int nLevel = 0;

			for (Exception e = ex; e != null; e = e.InnerException, nLevel++) {
				os.AppendFormat("Level {0} - exception message: {1}\n", nLevel, e.Message);
				os.AppendFormat(
					"Level {0} - exception stack trace begin:\n\n{1}\n\nLevel {0} - exception stack trace end\n",
					nLevel, e.StackTrace
				);
			} // for

			return os.ToString();
		} // ExceptionToString

		#endregion method ExceptionToString

		#region method OwnSay

		[StringFormatMethod("format")]
		protected abstract void OwnSay(Severity nSeverity, string format, params object[] parameters);
		[StringFormatMethod("format")]
		protected abstract void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters);

		#endregion method OwnSay

		#endregion protected

		#region private

		private ASafeLog m_oLog { get; set; }
		private static readonly object ms_oLock = new object();

		#endregion private
	} // class ASafeLog

	#endregion class ASafeLog
} // namespace Ezbob.Logger
