namespace Ezbob.Logger {
	using System;
	using log4net;
	using log4net.Appender;
	using log4net.Core;
	using log4net.Layout;
	using log4net.Repository.Hierarchy;
	using JetBrains.Annotations;

	public class LegacyLog : ASafeLog {
		static LegacyLog() {
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.RemoveAllAppenders();

			var fileAppender = new RollingFileAppender();
			fileAppender.AppendToFile = true;
			fileAppender.LockingModel = new FileAppender.MinimalLock();

			if (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Contains("iis"))
				fileAppender.File = System.Web.Hosting.HostingEnvironment.SiteName;
			else
				fileAppender.File = System.AppDomain.CurrentDomain.FriendlyName.Split('.')[0];

			fileAppender.StaticLogFileName = false;
			fileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
			fileAppender.DatePattern = "yyyyMMdd'.log'";
			fileAppender.Threshold = Level.Debug;

			var pl = new PatternLayout { ConversionPattern = "%d [%t] %-5level %message %n" };
			pl.ActivateOptions();

			fileAppender.Layout = pl;
			fileAppender.ActivateOptions();

			log4net.Config.BasicConfigurator.Configure(fileAppender);

			ms_oLog.Info("Initialized Logger");
		} // static constructor

		public LegacyLog(ASafeLog oLog = null) : base(oLog) { } // constructor

		public virtual ILog UnderlyingLog { get { return ms_oLog; } } // UnderlyingLog

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			switch (nSeverity) {
			case Severity.Debug:
				ms_oLog.Debug(string.Format(format, parameters));
				break;

			case Severity.Info:
			case Severity.Msg:
				ms_oLog.Info(string.Format(format, parameters));
				break;

			case Severity.Warn:
				ms_oLog.Warn(string.Format(format, parameters));
				break;

			case Severity.Error:
			case Severity.Alert:
				ms_oLog.Error(string.Format(format, parameters));
				break;

			case Severity.Fatal:
				ms_oLog.Fatal(string.Format(format, parameters));
				break;

			default:
				throw new ArgumentOutOfRangeException("nSeverity");
			} // switch
		} // OwnSay

		[StringFormatMethod("format")]
		protected override void OwnSay(Severity nSeverity, Exception ex, string format, params object[] parameters) {
			switch (nSeverity) {
			case Severity.Debug:
				ms_oLog.Debug(string.Format(format, parameters), ex);
				break;

			case Severity.Info:
			case Severity.Msg:
				ms_oLog.Info(string.Format(format, parameters), ex);
				break;

			case Severity.Warn:
				ms_oLog.Warn(string.Format(format, parameters), ex);
				break;

			case Severity.Error:
			case Severity.Alert:
				ms_oLog.Error(string.Format(format, parameters), ex);
				break;

			case Severity.Fatal:
				ms_oLog.Fatal(string.Format(format, parameters), ex);
				break;

			default:
				throw new ArgumentOutOfRangeException("nSeverity");
			} // switch
		} // OwnSay

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(ASafeLog));
	} // class LegacyLog
} // namespace Ezbob.Logger
