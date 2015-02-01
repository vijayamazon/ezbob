namespace EzServiceHost {
	using EzBob.CommonLib;
	using EzBob.PayPalServiceLib.Common;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
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

			bool isSalesForceProd = false;//TODO
			if (isSalesForceProd) {
				For<ISalesForceAppClient>().Use<SalesForceApiClient>();
			} else {
				For<ISalesForceAppClient>().Use<DummyApiClient>();
			}
		}
	}
}
