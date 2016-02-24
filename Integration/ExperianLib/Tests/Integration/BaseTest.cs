namespace ExperianLib.Tests.Integration {
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using log4net;
	using NHibernate;
	using NUnit.Framework;
	using NHibernateWrapper.NHibernate;
	using Ezbob.RegistryScanner;
	using EzServiceAccessor;
	using StructureMap;
	using StructureMap.Pipeline;

    [Ignore("Ignore this fixture")]
	[TestFixture]
	public class BaseTest {
		[SetUp]
		public void Start() {
			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
			Scanner.Register();
			ObjectFactory.Configure(x => {
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});

			m_oLog = new SafeILog(Log);

			this.oLog4NetCfg = new Log4Net().Init();

			m_oDB = new SqlConnection(oLog4NetCfg.Environment, m_oLog);

			ConfigManager.CurrentValues.Init(m_oDB, m_oLog);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
		} // Start

		protected AConnection m_oDB;
		protected ASafeLog m_oLog;

		protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseTest));
	    protected Log4Net oLog4NetCfg;
	} // class BaseTest
} // namespace
