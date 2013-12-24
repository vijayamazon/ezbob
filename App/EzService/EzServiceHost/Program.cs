using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using EKM;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonLib;
using EzBob.PayPal;
using EzBob.eBayLib;
using EzService;
using Ezbob.Database;
using Ezbob.Logger;
using FreeAgent;
using Integration.ChannelGrabberFrontend;
using NDesk.Options;
using NHibernate;
using PayPoint;
using Sage;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using StructureMap.Pipeline;
using YodleeLib.connector;
using log4net;
using Scorto.Configuration.Loader;
using ISession = NHibernate.ISession;

namespace EzServiceHost {
	public class Program : IHost {
		#region public

		#region method Shutdown

		public void Shutdown() {
			lock (ms_oLock) {
				ms_bStop = true;
			} // lock
		} // Handle

		#endregion method Shutdown

		#endregion public

		#region private

		#region method Main

		private static void Main(string[] args) {
			var app = new Program(args);

			if (app.Init())
				app.Run();

			app.Done();
		} // Main

		#endregion method Main

		#region method Usage

		private bool Usage(OptionSet oArgs, Exception e) {
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

			return false;
		} // Usage

		#endregion method Usage

		#region constructor

		private Program(string[] args) {
			m_aryArgs = args;

			log4net.Config.XmlConfigurator.Configure();

			m_oLog = new SafeILog(LogManager.GetLogger(typeof(EzServiceHost)));

			NotifyStartStop("started");

			EnvironmentConfigurationLoader.AppPathDummy = @"c:\ezbob\app\pluginweb\EzBob.Web\";

			NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
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

			Scanner.Register();

			ObjectFactory.Configure(x => {
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});
		} // constructor

		#endregion constructor

		#region method InitInstanceName

		private bool InitInstanceName() {
			m_sInstanceName = string.Empty;

			bool bShowHelp = false;

			var oArgs = new OptionSet {
				{ "n|name=", "this service host instance NAME (which is used to search DB)", v => m_sInstanceName = v },
				{ "h|help",  "show this message and exit", v => bShowHelp = v != null },
			};

			oArgs.Parse(m_aryArgs);

			if (bShowHelp)
				return Usage(oArgs, null);

			if (string.IsNullOrWhiteSpace(m_sInstanceName))
				return Usage(oArgs, new Exception("Instance name not specifed."));

			return true;
		} // InitInstanceName

		#endregion method InitInstanceName

		#region method Init

		private bool Init() {
			if (!InitInstanceName())
				return false;

			var env = new Ezbob.Context.Environment(m_oLog);
			m_oDB = new SqlConnection(env, m_oLog);

			if (m_oDB == null)
				throw new Exception("Failed to create a DB connection.");

			m_oCfg = new Configuration(m_sInstanceName, m_oDB, m_oLog);

			return true;
		} // Init

		#endregion method Init

		#region method Run

		private void Run() {
			try {
				var oHost = new EzServiceHost(
					new EzServiceInstanceRuntimeData { Host = this, Log = m_oLog, DB = m_oDB, InstanceName = m_sInstanceName },
					typeof(EzServiceImplementation),
					new Uri(m_oCfg.GetAdminEndpointAddress()),
					new Uri(m_oCfg.GetClientEndpointAddress())
				);

				SetMetadataEndpoit(oHost);

				oHost.AddServiceEndpoint(typeof(IEzServiceAdmin), new NetTcpBinding(), m_oCfg.GetAdminEndpointAddress());

				// To enable HTTP binding on custom port: open cmd.exe as administrator and
				//     netsh http add urlacl url=http://+:7082/ user=ALEXBO-PC\alexbo
				// where 7082 is your customer port and ALEXBO-PC\alexbo is the user
				// who runs the instance of the host.
				// To remove permission:
				//     netsh http add urlacl url=http://+:7082/
				// Mind the backslash at the end of the URL.
				oHost.AddServiceEndpoint(typeof(IEzService), new NetHttpBinding(), m_oCfg.GetClientEndpointAddress());

				m_oLog.Info("EzService endpoint has been created.");

				oHost.Open();

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

				oHost.Close();

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

		#region method SetMetadataEndpoint

		private void SetMetadataEndpoit(EzServiceHost oHost) {
			m_oLog.Info("Establishing EzService meta data publishing endpoint...");

			ServiceMetadataBehavior metadataBehavior = oHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

			if (metadataBehavior == null) {
				metadataBehavior = new ServiceMetadataBehavior { HttpGetEnabled = true };
				oHost.Description.Behaviors.Add(metadataBehavior);
			} // if

			Binding binding = MetadataExchangeBindings.CreateMexTcpBinding();
			oHost.AddServiceEndpoint(typeof(IMetadataExchange), binding, "MEX");

			m_oLog.Info("Establishing EzService meta data publishing endpoint completed.");
		} // SetMetadataEndpoint

		#endregion method SetMetadataEndpoint

		#region method NotifyStartStop

		private void NotifyStartStop(string sEvent) {
			m_oLog.Info(
				"Logging {0} for {1} v{5} on {2} as {3} with pid {4}.",
				sEvent,
				"EzService",
				System.Environment.MachineName,
				System.Environment.UserName,
				Process.GetCurrentProcess().Id,
				Assembly.GetCallingAssembly().GetName().Version.ToString(4)
			);
		} // NotifyStartStop

		#endregion method NotifyStartStop

		#region properties

		private readonly string[] m_aryArgs;
		private string m_sInstanceName;
		private Configuration m_oCfg;

		private ASafeLog m_oLog;
		private AConnection m_oDB;

		#endregion properties

		#region static properties

		private static bool ms_bStop = false;
		private static readonly object ms_oLock = new object();

		#endregion static properties

		#endregion private
	} // class Program
} // namespace EzServiceHost
