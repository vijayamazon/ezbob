using System;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Ezbob.Logger {
	#region class LegacyLog

	public class LegacyLog : ASafeLog {
		#region static constructor

		static LegacyLog() {
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.RemoveAllAppenders();

			var fileAppender = new RollingFileAppender();
			fileAppender.AppendToFile = true;
			fileAppender.LockingModel = new FileAppender.MinimalLock();

			if (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Contains("iis"))
				fileAppender.File = System.Web.Hosting.HostingEnvironment.SiteName; // System.AppDomain.CurrentDomain.FriendlyName.Split('.')[0];
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

		#endregion static constructor

		#region public

		#region constructor

		public LegacyLog(ASafeLog oLog = null) : base(oLog) { } // constructor

		#endregion constructor

		#region property UnderlyingLog

		public virtual ILog UnderlyingLog { get { return ms_oLog; } } // UnderlyingLog

		#endregion property UnderlyingLog

		#region method OwnSay

		public override void OwnSay(Severity nSeverity, string format, params object[] parameters) {
			switch (nSeverity) {
			case Severity.Debug:
				ms_oLog.Debug(string.Format(format, parameters));
				break;

			case Severity.Info:
				ms_oLog.Info(string.Format(format, parameters));
				break;

			case Severity.Warn:
				ms_oLog.Warn(string.Format(format, parameters));
				break;

			case Severity.Error:
				ms_oLog.Error(string.Format(format, parameters));
				break;

			case Severity.Fatal:
				ms_oLog.Fatal(string.Format(format, parameters));
				break;

			default:
				throw new ArgumentOutOfRangeException("nSeverity");
			} // switch
		} // OwnSay

		#endregion method OwnSay

		#endregion public

		#region private

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(ASafeLog));

		#endregion private
	} // class LegacyLog

	#endregion class LegacyLog
} // namespace Ezbob.Logger
