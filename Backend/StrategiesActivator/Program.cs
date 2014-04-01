namespace StrategiesActivator
{
	using System;
	using NHibernate;
	using Scorto.Configuration.Loader;
	using NHibernateWrapper.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;

	public class Program
    {
		public static void Main(string[] args)
        {
			log4net.Config.XmlConfigurator.Configure();

			if (args.Length < 2)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> <StrategyName> [param1] [param2] ... [paramN]");
				return;
			}

			LoadConfigurations();

			var strategiesActivator = new ServiceClientActivation(args);
			strategiesActivator.Execute();
        }

		private static void LoadConfigurations()
        {
			EnvironmentConfigurationLoader.AppPathDummy = @"c:\ezbob\app\pluginweb\EzBob.Web\";

			Scanner.Register();

			ObjectFactory.Configure(x =>
			{
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});
        }
    }
}
