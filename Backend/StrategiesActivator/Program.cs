namespace StrategiesActivator
{
	using System;
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using NHibernate;
	using PayPoint;
	using Sage;
	using Scorto.Configuration.Loader;
	using Scorto.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using YodleeLib.connector;

	public class Program
    {
		public static void Main(string[] args)
        {
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe <StrategyName> [param1] [param2] ... [paramN]");
				return;
			}

			LoadConfigurations();

			var strategiesActivator = new StrategiesActivator(args);
			strategiesActivator.Execute();
        }

		private static void LoadConfigurations()
        {
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

			ObjectFactory.Configure(x =>
			{
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});
        }
    }
}
