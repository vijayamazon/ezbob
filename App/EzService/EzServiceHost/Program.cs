namespace EzServiceHost {
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using CompanyFiles;
	using ConfigManager;
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EzBob.AmazonLib;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using EzService;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using NDesk.Options;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using PayPoint;
	using Sage;
	using StructureMap;
	using StructureMap.Pipeline;
	using YodleeLib.connector;
	using ISession = NHibernate.ISession;
	using EzBob.Configuration;
	using EzServiceConfigurationLoader;
	using ActionResult = Ezbob.Database.ActionResult;

	public class Program : IHost {
		#region public

		#region method Shutdown

		public void Shutdown() {
			lock (ms_oLock) {
				ms_bStop = true;
			} // lock

			GetAvailableFunds.StopBackgroundThread = true;
		} // Handle

		#endregion method Shutdown

		#endregion public

		#region private

		#region method Main

		private static void Main(string[] args) {
			var app = new Program(args);

			try {
				if (app.Init())
					app.Run();

				app.Done();
			}
			catch (Exception e) {
				app.m_oLog.Fatal(e, "EzServiceHost root level: unhandled exception caught!");
				throw;
			} // try
		} // Main

		#endregion method Main

		#region method Usage

		private void Usage(OptionSet oArgs, Exception e) {
			var os = new StringBuilder();
			var sw = new StringWriter(os);

			os.AppendLine("Usage: EzServiceHost [OPTIONS]");
			os.AppendLine("Options:");
			oArgs.WriteOptionDescriptions(sw);

			sw.Close();

			string s = os.ToString();

			Console.WriteLine(s);

			if (e == null)
				m_oLog.Msg(s);
			else {
				m_oLog.Msg(e, s);
				throw e;
			} // if
		} // Usage

		#endregion method Usage

		#region constructor

		private Program(string[] args) {
			m_aryArgs = args;
			m_oEnv = null;

			try {
				m_oEnv = new Ezbob.Context.Environment();
			}
			catch(Exception e) {
				throw new NullReferenceException("Failed to determine current environment.", e);
			} // try

			var oLog4NetCfg = new Log4Net(m_oEnv).Init();

			m_oLog = new SafeILog(this);

			NotifyStartStop("started");

			m_oLog.Debug("Current environment: {0}", m_oEnv.Context);
			m_oLog.Debug("Error emails will be sent to: {0}", oLog4NetCfg.ErrorMailRecipient);

			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(eBayDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(AmazonDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(PayPalDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(EkmDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(DatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(YodleeDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(PayPointDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(FreeAgentDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(SageDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(CompanyFilesDatabaseMarketPlace).Assembly);

			Ezbob.RegistryScanner.Scanner.Register();

			ObjectFactory.Configure(x => {
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>();
			});

			ObjectFactory.Configure(x => x.AddRegistry<EzBobRegistry>());
		} // constructor

		#endregion constructor

		#region method InitInstanceName

		private bool InitInstanceName() {
			m_sInstanceName = string.Empty;

			bool bShowHelp = false;

			var oArgs = new OptionSet {
				{ "n|name=", "this service host instance (NAME) (which is used to search DB)", v => m_sInstanceName = v },
				{ "h|help",  "show this message and exit", v => bShowHelp = v != null },
			};

			oArgs.Parse(m_aryArgs);

			if (bShowHelp) {
				Usage(oArgs, null);
				return false;
			} // if

			if (string.IsNullOrWhiteSpace(m_sInstanceName)) {
				m_oDB.ForEachRowSafe(
					(sr, bRowSetStarts) => {
						m_sInstanceName = sr["InstanceName"];
						return ActionResult.SkipAll;
					},
					"EzServiceGetDefaultInstance",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@Argument", System.Environment.MachineName)
				);
			} // if

			if (string.IsNullOrWhiteSpace(m_sInstanceName))
				Usage(oArgs, new Exception("Instance name not specifed."));

			return true;
		} // InitInstanceName

		#endregion method InitInstanceName

		#region method Init

		private bool Init() {
			m_oDB = new SqlConnection(m_oEnv, m_oLog);

			if (m_oDB == null)
				throw new Exception("Failed to create a DB connection.");

			if (!InitInstanceName())
				return false;

			EzServiceAccessorShort.Set(m_oDB, m_oLog);

			m_oCfg = new Configuration(m_sInstanceName, m_oDB, m_oLog);

			m_oCfg.Init();

			m_oHost = new EzServiceHost(m_oCfg, new EzServiceInstanceRuntimeData {
				Host = this,
				Log = m_oLog,
				DB = m_oDB,
				InstanceName = m_sInstanceName,
				InstanceID = m_oCfg.InstanceID
			});

			CurrentValues.ReloadOnTimer oOnTimer = () => {
				DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
				AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
			};

			CurrentValues.Init(m_oDB, m_oLog);
			CurrentValues.Instance.RefreshIntervalMinutes = CurrentValues.Instance.EzServiceUpdateConfiguration;
			oOnTimer();
			CurrentValues.OnReloadByTimer += oOnTimer;

			return true;
		} // Init

		#endregion method Init

		#region method Run

		private void Run() {
			try {
				m_oHost.Open();

				m_oLog.Info("EzService host has been opened.");

				m_oLog.Info("Entering the main loop.");

				bool bStop = false;

				do {
					Thread.Sleep(m_oCfg.SleepTimeout);

					lock (ms_oLock) {
						bStop = ms_bStop;
					} // lock
				} while (!bStop);

				m_oLog.Info("Main loop has completed.");

				m_oHost.Close();

				m_oLog.Info("EzService host has been closed.");
			}
			catch (Exception e) {
				m_oLog.Error(e, "Unhandled exception caught!");
			} // try
		} // Run

		#endregion method Run

		#region method Done

		private void Done() {
			if (m_oDB != null) {
				m_oDB.Dispose();
				m_oDB = null;
			} // if

			NotifyStartStop("stopped");
		} // Done

		#endregion method Done

		#region method NotifyStartStop

		private void NotifyStartStop(string sEvent) {
			m_oLog.Info(
				"Logging {0} for {1} v{5} on {2} as {3} with pid {4}.",
				sEvent,
				"EzService",
				Environment.MachineName,
				Environment.UserName,
				Process.GetCurrentProcess().Id,
				Assembly.GetCallingAssembly().GetName().Version.ToString(4)
			);
		} // NotifyStartStop

		#endregion method NotifyStartStop

		#region properties

		private readonly string[] m_aryArgs;
		private string m_sInstanceName;
		private Configuration m_oCfg;
		private EzServiceHost m_oHost;

		private readonly Ezbob.Context.Environment m_oEnv;
		private readonly ASafeLog m_oLog;
		private AConnection m_oDB;

		#endregion properties

		#region static properties

		private static bool ms_bStop = false;
		private static readonly object ms_oLock = new object();

		#endregion static properties

		#endregion private
	} // class Program
} // namespace EzServiceHost
