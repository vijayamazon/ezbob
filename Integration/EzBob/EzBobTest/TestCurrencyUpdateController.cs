namespace EzBobTest
{
	using System.IO;
	using System.Xml;
	using CustomSchedulers.Currency;
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using NUnit.Framework;
	using PayPoint;
	using Sage;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using NHibernateWrapper.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using YodleeLib.connector;
	using log4net.Config;

	[TestFixture]
	public class TestCurrencyUpdateController
	{
		[SetUp]
		public void Init()
		{
			var paths = new string[] {
				@"c:\alexbo\src\App\clients\Maven\maven.exe",
				@"c:\EzBob\App\clients\Maven\maven.exe"
			};

			foreach (string sPath in paths)
			{
				if (File.Exists(sPath))
				{
					EnvironmentConfigurationLoader.AppPathDummy = sPath;
					break;
				} // if
			} // foreach

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
			ObjectFactory.Configure(x =>
			{
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});

			var cfg = ConfigurationRoot.GetConfiguration();

			XmlElement configurationElement = cfg.XmlElementLog;
			XmlConfigurator.Configure(configurationElement);
		}

		[Test]
		public void TestCurrencyUpdateControllerRun()
		{
			CurrencyUpdateController.Run();
		}
	}
}