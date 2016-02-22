namespace EzBobTest
{
	using System.IO;
	using System.Xml;
	using CustomSchedulers.Currency;
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using NUnit.Framework;
	using PayPoint;
	using Sage;
	using NHibernateWrapper.NHibernate;
	using Ezbob.RegistryScanner;
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
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});
		}

		[Test]
		public void TestCurrencyUpdateControllerRun()
		{
			CurrencyUpdateController.Run();
		}
	}
}