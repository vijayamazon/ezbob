namespace EzBob.Configuration
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using MailApi;
	using PayPalServiceLib.Common;
	using StructureMap.Configuration.DSL;

    public class EzBobRegistry : Registry
	{
		public EzBobRegistry()
		{
			For<IServiceEndPointFactory>().Use( new ServiceEndPointFactory() );

			For<IDatabaseDataHelper>().Use<DatabaseDataHelper>();
			For<IBugRepository>().Use<BugRepository>();
			For<ICustomerStatusesRepository>().Use<CustomerStatusesRepository>();
			For<IApprovalsWithoutAMLRepository>().Use<ApprovalsWithoutAMLRepository>();
			For<IMail>().Use<Mail>();
			For<ILoanRepository>().Use<LoanRepository>();
			For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
			For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
		}
	}
}
