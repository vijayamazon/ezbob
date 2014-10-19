namespace StrategiesActivator {
	using System;
	using System.Net;
	using EzServiceAccessor;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using Ezbob.RegistryScanner;
	using ServiceClientProxy;
	using StructureMap;
	using StructureMap.Pipeline;

	public class Program {
		public static void Main(string[] args) {
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
			log4net.Config.XmlConfigurator.Configure();

			if (args.Length < 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe [<Service Instance Name>] <StrategyName> [param1] [param2] ... [paramN]");
				return;
			} // if

			Scanner.Register();

			ObjectFactory.Configure(x => {
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				x.For<IEzServiceAccessor>().Use<EzServiceAccessorLong>();
			});

			try {
				var strategiesActivator = new ServiceClientActivation(args);
				strategiesActivator.Execute();
			}
			catch (ExitException) {
				// do nothing here
			} // try
		} // Main
	} // class Program
} // namespace
