namespace EzBob.Web.Infrastructure
{
	using System.Web.Security;
	using ApplicationMng.Repository;
	using Code.Bank;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Email;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models.Agreements;
	using Membership;
	using PayPalServiceLib.Common;
	using Areas.Customer.Controllers;
	using Areas.Customer.Models;
	using Code;
	using Code.Agreements;
	using Code.Email;
	using Code.MpUniq;
	using Models.Repository;
	using MailApi;
	using NHibernateWrapper.Web;
	using StructureMap.Configuration.DSL;
	using ConfigManager;

	public class PluginWebRegistry : Registry
	{
		public PluginWebRegistry()
		{
			For<MembershipProvider>().Use<EzbobMembershipProvider>();
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

			For<IServiceEndPointFactory>().Use(new ServiceEndPointFactory());
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
			
			bool payPointValidateName = CurrentValues.Instance.PayPointValidateName;
			if (payPointValidateName)
			{
				For<ICustomerNameValidator>().Use<CustomerNameValidator>();
			}
			else
			{
				For<ICustomerNameValidator>().Use<FakeCustomerNameValidator>();
			}

			if (CurrentValues.Instance.CheckStoreUniqueness)
			{
				For<IMPUniqChecker>().Use<MPUniqChecker>();
			}
			else
			{
				For<IMPUniqChecker>().Use<FakeMPUniqChecker>();
			}

			For<IEmailConfirmationRequestRepository>().Use<EmailConfirmationRequestRepository>();
			For<IEmailConfirmation>().Use<EmailConfirmation>();
			For<IDecisionHistoryRepository>().Use<DecisionHistoryRepository>();
			
			For<IYodleeAccountChecker>().Use<YodleeAccountChecker>();

			For<ILoanCreator>().Use<LoanCreator>();
			For<IAgreementsTemplatesProvider>().Use<AgreementsTemplatesProvider>();
			For<ILoanTypeRepository>().Use<LoanTypeRepository>();
			For<IPaypointTransactionRepository>().Use<PaypointTransactionRepository>();
			For<IExperianConsentAgreementRepository>().Use<ExperianConsentAgreementRepository>();
			For<ICashRequestRepository>().Use<CashRequestRepository>();
			For<IConfigurationVariablesRepository>().Use<ConfigurationVariablesRepository>();
			For<ILoanChangesHistoryRepository>().Use<LoanChangesHistoryRepository>();
			For<IPacNetBalanceRepository>().Use<PacNetBalanceRepository>();
			For<IPacNetManualBalanceRepository>().Use<PacNetManualBalanceRepository>();
			For<IDiscountPlanRepository>().Use<DiscountPlanRepository>();
			For<ICurrencyConvertor>().Use<CurrencyConvertor>();
			For<IMail>().Use<Mail>();
			For<ICustomerSessionsRepository>().Use<CustomerSessionsRepository>();
			For<ITestCustomerRepository>().Use<TestCustomerRepository>();
			For<ICustomerStatusesRepository>().Use<CustomerStatusesRepository>();
			For<IApprovalsWithoutAMLRepository>().Use<ApprovalsWithoutAMLRepository>();
			For<ICustomerStatusHistoryRepository>().Use<CustomerStatusHistoryRepository>();
			For<ILoanSourceRepository>().Use<LoanSourceRepository>();

			For<ICustomerReasonRepository>().Use<CustomerReasonRepository>();
			For<ICustomerSourceOfRepaymentRepository>().Use<CustomerSourceOfRepaymentRepository>();
			For<ICustomerInviteFriendRepository>().Use<CustomerInviteFriendRepository>();
			For<IWhatsNewCustomerMapRepository>().Use<WhatsNewCustomerMapRepository>();
			For<IWhatsNewRepository>().Use<WhatsNewRepository>();

			For<IUsersRepository>().Use<UsersRepository>();
			For<IRolesRepository>().Use<RolesRepository>();
			For<ISecurityQuestionRepository>().Use<SecurityQuestionRepository>();
		}
	}
}