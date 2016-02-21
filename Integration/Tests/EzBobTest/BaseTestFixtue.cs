namespace EzBobTest {
	using ConfigManager;
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using Ezbob.RegistryScanner;
	using EzServiceAccessor;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using NUnit.Framework;
	using PayPoint;
	using Sage;
	using StructureMap;
	using StructureMap.Pipeline;
	using YodleeLib.connector;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using ServiceClientProxy;

	[TestFixture]
	public class BaseTestFixtue {
		[SetUp]
		public void Init() {
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

			Scanner.Register();

			ObjectFactory.Configure(x => {
				x.For<ISession>()
				 .LifecycleIs(new ThreadLocalStorageLifecycle())
				 .Use(ctx => NHibernateManager.OpenSession());

				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				x.For<ICustomerRepository>().Use<CustomerRepository>();
				x.For<ILoanLegalRepository>().Use<LoanLegalRepository>();
				x.For<ILoanTypeRepository>().Use<LoanTypeRepository>();
				x.For<ILoanSourceRepository>().Use<LoanSourceRepository>();
				x.For<IDiscountPlanRepository>().Use<DiscountPlanRepository>();
				x.For<ILoanOptionsRepository>().Use<LoanOptionsRepository>();
				x.For<IEzServiceAccessor>().Use<EzServiceAccessorLong>();
			});

			var oLog4NetCfg = new Log4Net().Init();

			m_oEnv = oLog4NetCfg.Environment;

			m_oLog = new ConsoleLog(new SafeILog(this));

			m_oDB = new SqlConnection(oLog4NetCfg.Environment, m_oLog);

			ConfigManager.CurrentValues.Init(m_oDB, m_oLog);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);

			Ezbob.Backend.Strategies.Library.Initialize(m_oEnv, m_oDB, m_oLog);
		} // Init

		protected AConnection m_oDB;
		public ASafeLog m_oLog { get; protected set; }
		protected Ezbob.Context.Environment m_oEnv;
	} // class BaseTestFixture
} // namespace
