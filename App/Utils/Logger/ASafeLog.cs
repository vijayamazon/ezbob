using System;
using System.Globalization;

namespace Ezbob.Logger {
	#region class ASafeLog

	public abstract class ASafeLog {
		#region public

		#region method Say

		public virtual void Say(Severity nSeverity, object obj) { Say(nSeverity, (obj ?? "<null>").ToString()); } // Say
		public virtual void Say(Severity nSeverity, string message) { Say(nSeverity, "{0}", message ?? "<null>"); } // Say
		public virtual void Say(Severity nSeverity, string format, params object[] parameters) {
			OwnSay(nSeverity, format, parameters);

			lock (ms_oLock) {
				if (m_oLog != null)
					m_oLog.Say(nSeverity, format, parameters);
			} // lock
		} // Say

		#endregion method Say

		public abstract void OwnSay(Severity nSeverity, string format, params object[] parameters);

		#region method Debug

		public virtual void Debug(object obj) { Debug((obj ?? "<null>").ToString()); } // Debug
		public virtual void Debug(string message) { Debug("{0}", message ?? "<null>"); } // Debug
		public virtual void Debug(string format, params object[] parameters) { Say(Severity.Debug, format, parameters); } // Debug

		#endregion method Debug

		#region method Info

		public virtual void Info(object obj) { Info((obj ?? "<null>").ToString()); } // Info
		public virtual void Info(string message) { Info("{0}", message ?? "<null>"); } // Info
		public virtual void Info(string format, params object[] parameters) { Say(Severity.Info, format, parameters); } // Info

		#endregion method Info

		#region method Warn

		public virtual void Warn(object obj) { Warn((obj ?? "<null>").ToString()); } // Warn
		public virtual void Warn(string message) { Warn("{0}", message ?? "<null>"); } // Warn
		public virtual void Warn(string format, params object[] parameters) { Say(Severity.Warn, format, parameters); } // Warn

		#endregion method Warn

		#region method Error

		public virtual void Error(object obj) { Error((obj ?? "<null>").ToString()); } // Error
		public virtual void Error(string message) { Error("{0}", message ?? "<null>"); } // Error
		public virtual void Error(string format, params object[] parameters) { Say(Severity.Error, format, parameters); } // Error

		#endregion method Error

		#region method Fatal

		public virtual void Fatal(object obj) { Fatal((obj ?? "<null>").ToString()); } // Fatal
		public virtual void Fatal(string message) { Fatal("{0}", message ?? "<null>"); } // Fatal
		public virtual void Fatal(string format, params object[] parameters) { Say(Severity.Fatal, format, parameters); } // Fatal

		#endregion method Fatal

		#endregion public

		#region protected

		#region constructor

		protected ASafeLog(ASafeLog oLog = null) {
			m_oLog = oLog;
		} // constructor

		#endregion constructor

		#region property CurrentTime

		protected virtual string CurrentTime {
			get { return DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.ffffff", CultureInfo.InvariantCulture); }
		} // CurrentTime

		#endregion property CurrentTime

		#endregion protected

		#region private

		private ASafeLog m_oLog { get; set; }
		private static string ms_oLock = "";

		#endregion private
	} // class ASafeLog

	#endregion class ASafeLog
} // namespace Ezbob.Logger
