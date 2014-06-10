namespace EzBobTest
{
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.RegistryScanner;
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
	using log4net;

	[TestFixture]
	public class BaseTestFixtue
	{
		[SetUp]
		public void Init()
		{
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
			ObjectFactory.Configure(x =>
			{
				x.For<ISession>()
				 .LifecycleIs(new ThreadLocalStorageLifecycle())
				 .Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});

			m_oLog = new SafeILog(LogManager.GetLogger(typeof(TestStrategies)));

			var env = new Ezbob.Context.Environment(m_oLog);
			m_oDB = new SqlConnection(env, m_oLog);
			ConfigManager.CurrentValues.Init(m_oDB, m_oLog);
		}

		protected AConnection m_oDB;
		protected ASafeLog m_oLog;
	}
}
