namespace StrategiesActivator {
	using System;
	using EzServiceAccessor;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using Ezbob.RegistryScanner;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using ServiceClientProxy;
	using StructureMap;
	using StructureMap.Pipeline;

	public class Program {
		public static void Main(string[] args) {
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
				x.For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
				x.For<ILoanTransactionMethodRepository>().Use<LoanTransactionMethodRepository>();
				x.For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
				x.For<ILoanOptionsRepository>().Use<LoanOptionsRepository>();
				x.For<ICustomerRepository>().Use<CustomerRepository>();
				x.For<ILoanLegalRepository>().Use<LoanLegalRepository>();
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
