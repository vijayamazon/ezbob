using System;
using log4net;

namespace Ezbob.Logger {
	#region class SafeILog

	public class SafeILog : ASafeLog {
		#region public

		#region constructor

		public SafeILog(ILog oiLog, ASafeLog oLog = null) : base(oLog) {
			m_oiLog = oiLog;
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method OwnSay

		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			if (m_oiLog == null)
				return;

			switch (nSeverity) {
			case Severity.Debug:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Debug(format);
				else
					m_oiLog.DebugFormat(format, parameters);

				break;

			case Severity.Info:
			case Severity.Msg:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Info(format);
				else
					m_oiLog.InfoFormat(format, parameters);

				break;

			case Severity.Warn:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Warn(format);
				else
					m_oiLog.WarnFormat(format, parameters);

				break;

			case Severity.Error:
			case Severity.Alert:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Error(format);
				else
					m_oiLog.ErrorFormat(format, parameters);

				break;

			case Severity.Fatal:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Fatal(format);
				else
					m_oiLog.FatalFormat(format, parameters);

				break;

			default:
				throw new ArgumentOutOfRangeException("nSeverity");
			} // switch
		} // OwnSay

		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			if (m_oiLog == null)
				return;

			switch (nSeverity) {
			case Severity.Debug:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Debug(format, ex);
				else {
					m_oiLog.DebugFormat(format, parameters);
					m_oiLog.Debug("", ex);
				} // if

				break;

			case Severity.Info:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Info(format);
				else {
					m_oiLog.InfoFormat(format, parameters);
					m_oiLog.Info("", ex);
				} // if

				break;

			case Severity.Warn:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Warn(format);
				else {
					m_oiLog.WarnFormat(format, parameters);
					m_oiLog.Warn("", ex);
				} // if

				break;

			case Severity.Error:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Error(format);
				else {
					m_oiLog.ErrorFormat(format, parameters);
					m_oiLog.Error("", ex);
				} // if

				break;

			case Severity.Fatal:
				if ((parameters == null) || (parameters.Length == 0))
					m_oiLog.Fatal(format);
				else {
					m_oiLog.FatalFormat(format, parameters);
					m_oiLog.Fatal("", ex);
				} // if

				break;

			default:
				throw new ArgumentOutOfRangeException("nSeverity");
			} // switch
		} // OwnSay

		#endregion method OwnSay

		#endregion protected

		#region private

		private readonly ILog m_oiLog;

		#endregion private
	} // class SafeILog

	#endregion class SafeILog
} // namespace Ezbob.Logger
