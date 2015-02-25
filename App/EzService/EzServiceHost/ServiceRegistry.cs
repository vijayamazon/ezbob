namespace EzServiceHost {
	using ConfigManager;
	using EzBob.CommonLib;
	using EzBob.PayPalServiceLib.Common;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using SalesForceLib;
	using StructureMap.Configuration.DSL;

	public class ServiceRegistry : Registry {
		public ServiceRegistry() {
			For<IEzServiceAccessor>().Use<EzServiceAccessorShort>();

			For<IServiceEndPointFactory>().Use(new ServiceEndPointFactory());
			For<IDatabaseDataHelper>().Use<DatabaseDataHelper>();
			For<IBugRepository>().Use<BugRepository>();
			For<ICustomerStatusesRepository>().Use<CustomerStatusesRepository>();
			For<IApprovalsWithoutAMLRepository>().Use<ApprovalsWithoutAMLRepository>();
			For<ILoanRepository>().Use<LoanRepository>();
			For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
			For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
			For<ILoanTransactionMethodRepository>().Use<LoanTransactionMethodRepository>();
		
			For<ICustomerRepository>().Use<CustomerRepository>();
			For<ILoanTypeRepository>().Use<LoanTypeRepository>();
			For<ILoanSourceRepository>().Use<LoanSourceRepository>();
			For<IDiscountPlanRepository>().Use<DiscountPlanRepository>();


			
			if (CurrentValues.Instance.SalesForceFakeMode) {
				For<ISalesForceAppClient>().Use<FakeApiClient>();
			} else {
				For<ISalesForceAppClient>().Use<SalesForceApiClient>();
			}
		}
	}
}
