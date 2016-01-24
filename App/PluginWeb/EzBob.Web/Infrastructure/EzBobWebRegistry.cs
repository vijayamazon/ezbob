namespace EzBob.Web.Infrastructure
{
    using ConfigManager;
    using EzBob.CommonLib;
    using EzBob.PayPalServiceLib.Common;
    using EzBob.Web.Areas.Customer.Controllers;
    using EzBob.Web.Areas.Customer.Models;
    using EzBob.Web.Code;
    using EzBob.Web.Code.Bank;
    using EzBob.Web.Code.MpUniq;
    using EzBob.Web.Models.Repository;
    using EzServiceAccessor;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using EZBob.DatabaseLib.Model.Database.Repository;
    using EZBob.DatabaseLib.Model.Database.UserManagement;
    using EZBob.DatabaseLib.Model.Experian;
    using EZBob.DatabaseLib.Model.Loans;
    using EZBob.DatabaseLib.Repository;
    using PostcodeAnywhere;
    using ServiceClientProxy;
    using StructureMap.Configuration.DSL;

    public class PluginWebRegistry : Registry
	{
		public PluginWebRegistry()
		{
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
			For<IBankAccountWhiteListRepository>().Use<BankAccountWhiteListRepository>();
			For<ICardInfoRepository>().Use<CardInfoRepository>();
			For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
			For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
			For<ILoanTransactionRepository>().Use<LoanTransactionRepository>();
			For<ILoanAgreementRepository>().Use<LoanAgreementRepository>();

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

			For<IDecisionHistoryRepository>().Use<DecisionHistoryRepository>();
			
			For<IYodleeAccountChecker>().Use<YodleeAccountChecker>();

			For<ILoanCreator>().Use<LoanCreator>();
			For<ILoanTypeRepository>().Use<LoanTypeRepository>();
			For<IPaypointTransactionRepository>().Use<PaypointTransactionRepository>();
			For<IExperianConsentAgreementRepository>().Use<ExperianConsentAgreementRepository>();
			For<ICashRequestRepository>().Use<CashRequestRepository>();
			For<ILoanChangesHistoryRepository>().Use<LoanChangesHistoryRepository>();
			For<IDiscountPlanRepository>().Use<DiscountPlanRepository>();
			For<ICurrencyConvertor>().Use<CurrencyConvertor>();
			For<ICustomerSessionsRepository>().Use<CustomerSessionsRepository>();
			For<ITestCustomerRepository>().Use<TestCustomerRepository>();
			For<ICustomerStatusesRepository>().Use<CustomerStatusesRepository>();
			For<IApprovalsWithoutAMLRepository>().Use<ApprovalsWithoutAMLRepository>();
			For<ICustomerStatusHistoryRepository>().Use<CustomerStatusHistoryRepository>();
			For<ILoanSourceRepository>().Use<LoanSourceRepository>();

			For<ICustomerReasonRepository>().Use<CustomerReasonRepository>();
			For<ICustomerSourceOfRepaymentRepository>().Use<CustomerSourceOfRepaymentRepository>();
			For<IVipRequestRepository>().Use<VipRequestRepository>();
			For<IWhatsNewCustomerMapRepository>().Use<WhatsNewCustomerMapRepository>();
			For<IWhatsNewRepository>().Use<WhatsNewRepository>();
			For<IWhatsNewExcludeCustomerOriginRepository>().Use<WhatsNewExcludeCustomerOriginRepository>();

			For<IUsersRepository>().Use<UsersRepository>();
			For<IRolesRepository>().Use<RolesRepository>();
			For<ISecurityQuestionRepository>().Use<SecurityQuestionRepository>();
			For<ISuggestedAmountRepository>().Use<SuggestedAmountRepository>();
			For<IEzServiceAccessor>().Use<EzServiceAccessorLong>();
			For<IExperianHistoryRepository>().Use<ExperianHistoryRepository>();

			For<IDatabaseDataHelper>().Use<DatabaseDataHelper>();
			For<IBugRepository>().Use<BugRepository>();
			For<IExternalCollectionStatusesRepository>().Use<ExternalCollectionStatusesRepository>();

			if (CurrentValues.Instance.PostcodeAnywhereEnabled) {
				For<ISortCodeChecker>().Use<SortCodeChecker>().Ctor<int>("maxBankAccountValidationAttempts").Is(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts);
			} else {
				For<ISortCodeChecker>().Use<FakeSortCodeChecker>().Ctor<int>("maxBankAccountValidationAttempts").Is(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts);
			}
		}
	}
}