namespace EzBobTest
{
	using System.Xml;
	using EKM;
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.MailStrategies;
	using FreeAgent;
	using Sage;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.CommonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using NHibernate;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using log4net.Config;
    using PayPoint;
    using YodleeLib.connector;
	using System.IO;
	using Integration.ChannelGrabberFrontend;

	[TestFixture]
	public class TestUpdateMarketplaces
	{
		private DatabaseDataHelper _Helper;

		[SetUp]
		public void Init()
		{
			var paths = new string[]
				{
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

			NHibernateManager.FluentAssemblies.Add(typeof (ApplicationMng.Model.Application).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (Customer).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (eBayDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (AmazonDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (PayPalDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (EkmDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (DatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (YodleeDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (PayPointDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (FreeAgentDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (SageDatabaseMarketPlace).Assembly);
			Scanner.Register();
			ObjectFactory.Configure(x =>
				{
					x.For<ISession>()
					 .LifecycleIs(new ThreadLocalStorageLifecycle())
					 .Use(ctx => NHibernateManager.SessionFactory.OpenSession());
					x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				});

			var cfg = ConfigurationRoot.GetConfiguration();

			XmlElement configurationElement = cfg.XmlElementLog;
			XmlConfigurator.Configure(configurationElement);

			_Helper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
		}

		[Test]
		public void UpdateCustomerMarketplace()
		{
			var s = new UpdateMarketplaces();
			s.CustomerMarketPlaceAddedAlternative(3055, 3040);
		}

		[Test]
		public void Greeting()
		{
			var s = new Greeting(3060, "dfgdfsg");
			s.Execute();
		}
	}
}