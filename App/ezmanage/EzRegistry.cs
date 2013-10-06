namespace ezmanage
{
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Email;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.CommonLib;
	using EzBob.Web.ApplicationCreator;
	using EzBob.Web.Areas.Customer.Controllers;
	using EzBob.Web.Areas.Customer.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Code.Agreements;
	using EzBob.Web.Code.Email;
	using EzBob.Models.Agreements;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Models.Repository;
	using NHibernate;
	using PaymentServices.PacNet;
	using Scorto.Web;
	using StructureMap.Configuration.DSL;

	public class EzRegistry: Registry
    {
        public EzRegistry()
        {
            var session = DBConfig.OpenSession();
            For<ISession>().Use(session);
            For<ICustomerRepository>().Use <CustomerRepository>();

            For<IWorkplaceContext>().Use<EzBobContext>();
            For<IEzbobWorkplaceContext>().Use<EzBobContext>();
            For<ICustomerRepository>().Use<CustomerRepository>();
            For<ICashRequestsRepository>().Use<CashRequestsRepository>();
            For<IPerformencePerUnderwriterReportRepository>().Use<PerformencePerUnderwriterReportRepository>();
            For<IPerformencePerMedalReportRepository>().Use<PerformencePerMedalReportRepository>();
            For<IExposurePerMedalReportRepository>().Use<ExposurePerMedalReportRepository>();
            For<IExposurePerUnderwriterReportRepository>().Use<ExposurePerUnderwriterReportRepository>();
            For<IMedalStatisticReportRepository>().Use<MedalStatisticReportRepository>();
            For<IDailyReportRepository>().Use<DailyReportRepository>();

            For<IAppCreator>().Use<FakeAppCreator>();
            For<IDbStringRepository>().Use<DbStringRepository>();
            
            For<IPayPointFacade>().Use<PayPointFacade>();
            For<IPersonalInfoHistoryRepository>().Use<PersonalInfoHistoryRepository>();
            For<IPacnetPaypointServiceLogRepository>().Use<PacnetPaypointServiceLogRepository>();
            For<ICustomerMarketPlaceRepository>().Use<CustomerMarketPlaceRepository>();
            For<IMP_WhiteListRepository>().Use<MP_WhiteListRepository>();
            For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
            For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
            For<ILoanTransactionRepository>().Use<LoanTransactionRepository>();
            For<ILoanAgreementRepository>().Use<LoanAgreementRepository>();
            For<ILoanAgreementTemplateRepository>().Use<LoanAgreementTemplateRepository>();

            For<ILoanRepository>().Use<LoanRepository>();
            For<IAgreementsGenerator>().Use<AgreementsGenerator>();
            For<ILoanOptionsRepository>().Use<LoanOptionsRepository>();

            For<ICustomerNameValidator>().Use<FakeCustomerNameValidator>();

            For<IEmailConfirmationRequestRepository>().Use<EmailConfirmationRequestRepository>();
            For<IEmailConfirmation>().Use <EmailConfirmation>();
            For<IDecisionHistoryRepository>().Use<DecisionHistoryRepository>();
			
            For<ILoanCreator>().Use<LoanCreator>();

            For<IPacnetService>().Use<FakePacnetService>();


            var config = new EzBobConfiguration();
            For<IEzBobConfiguration>().Singleton().Use(config);
            For<IAgreementsTemplatesProvider>().Use<EzAgreementsTemplateProvider>();
            For<ISecurityQuestionRepository>().Use<SecurityQuestionRepository>();
			For<ICustomerReasonRepository>().Use<CustomerReasonRepository>();
			For<ICustomerSourceOfRepaymentRepository>().Use<CustomerSourceOfRepaymentRepository>();
            For<IUsersRepository>().Use<UsersRepository>();
			For<IDatabaseDataHelper>().Use<DatabaseDataHelper>();
			For<IConfigurationVariablesRepository>().Use<ConfigurationVariablesRepository>();
        }
    }
}