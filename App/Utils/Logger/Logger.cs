namespace Logger
{
    using log4net;
    using log4net.Core;
    using log4net.Appender;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;

    public class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));

        static Logger()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders();

            var fileAppender = new RollingFileAppender();
            fileAppender.AppendToFile = true;
            fileAppender.LockingModel = new FileAppender.MinimalLock();
            if (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Contains("iis"))
            {
                fileAppender.File = System.Web.Hosting.HostingEnvironment.SiteName;// System.AppDomain.CurrentDomain.FriendlyName.Split('.')[0];
            }
            else
            {
                fileAppender.File = System.AppDomain.CurrentDomain.FriendlyName.Split('.')[0];
            }
            fileAppender.StaticLogFileName = false;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
            fileAppender.DatePattern = "yyyyMMdd'.log'";
            fileAppender.Threshold = Level.Debug;
            var pl = new PatternLayout { ConversionPattern = "%d [%t] %-5level %message %n" };
            pl.ActivateOptions();
            fileAppender.Layout = pl;
            fileAppender.ActivateOptions();

            log4net.Config.BasicConfigurator.Configure(fileAppender);

            Info("Initialized Logger");
        }

        public static void Debug(string message)
        {
            log.Debug(message);
        }

        public static void DebugFormat(string format, params object[] parameters)
        {
            log.Debug(string.Format(format, parameters));
        }

        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void InfoFormat(string format, params object[] parameters)
        {
            log.Info(string.Format(format, parameters));
        }

        public static void Warn(string message)
        {
            log.Warn(message);
        }

        public static void WarnFormat(string format, params object[] parameters)
        {
            log.Warn(string.Format(format, parameters));
        }

        public static void Error(string message)
        {
            log.Error(message);
        }

        public static void ErrorFormat(string format, params object[] parameters)
        {
            log.Error(string.Format(format, parameters));
        }

        public static void Fatal(string message)
        {
            log.Fatal(message);
        }

        public static void FatalFormat(string format, params object[] parameters)
        {
            log.Fatal(string.Format(format, parameters));
        }

        public static void Debug(object obj)
        {
            log.Debug(obj);
        }

        public static void Info(object obj)
        {
            log.Info(obj);
        }

        public static void Warn(object obj)
        {
            log.Warn(obj);
        }

        public static void Error(object obj)
        {
            log.Error(obj);
        }

        public static void Fatal(object obj)
        {
            log.Fatal(obj);
        }
    }
}
