using PayPal.Platform.SDK;

namespace EzBob.Web.Infrastructure
{
	using System.Web.Security;
	using ApplicationMng.Model.Commands.Player;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Email;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using AmazonServiceLib.Config;
	using Configuration;
	using PayPalServiceLib;
	using PayPalServiceLib.Common;
	using ApplicationCreator;
	using Areas.Customer.Controllers;
	using Areas.Customer.Models;
	using Code;
	using Code.Agreements;
	using Code.ApplicationCreator;
	using Code.Email;
	using Code.MpUniq;
	using Code.PostCode;
	using Models.Repository;
	using eBayLib.Config;
	using MailApi;
	using PostcodeAnywhere;
	using Scorto.Configuration;
	using Scorto.Web;
	using StructureMap.Configuration.DSL;
	using ZohoCRM;
	using Sage.Config;
	using FreeAgent.Config;
	using YodleeLib.config;

    public class PluginWebRegistry : Registry
    {
        public PluginWebRegistry()
        {
            var localRoot = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<EzBobConfigRoot>();
            var ezBobConfiguration = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<EzBobConfiguration>();
            var bobconfig = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRootBob>();

            For<ConfigurationRootBob>().Use(bobconfig);

            For<MembershipProvider>().Use<ScortoMembershipProvider>();
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
            For<IPlayer>().Use<PlayerDummy>();

            For<IEzBobConfiguration>().Singleton().Use(ezBobConfiguration);
            For<IPayPalConfig>().Singleton().Use(localRoot.PayPalConfig);
            For<IAppCreator>().Use<AppCreator>();
            For<BaseAPIProfile>().Use(() => ProfileProvider.CreateProfile(localRoot.PayPalConfig));
            For<IEbayMarketplaceTypeConnection>().Use(localRoot.eBayConfig);
            For<IEbayMarketplaceSettings>().Use(localRoot.eBaySettings);
			For<IAmazonMarketPlaceTypeConnection>().Use(localRoot.AmazonConfig);
			For<IYodleeMarketPlaceConfig>().Singleton().Use(localRoot.YodleeConfig);
			For<IFreeAgentConfig>().Singleton().Use(localRoot.FreeAgentConfig);
			For<ISageConfig>().Singleton().Use(localRoot.SageConfig);
            For<IServiceEndPointFactory>().Use(new ServiceEndPointFactory());
            For<IDbStringRepository>().Use<DbStringRepository>();
            For<EzBobConfigRoot>().Use(c => localRoot);
            For<IPayPointFacade>().Use<PayPointFacade>();
            For<IPersonalInfoHistoryRepository>().Use<PersonalInfoHistoryRepository>();
            For<IPacnetPaypointServiceLogRepository>().Use<PacnetPaypointServiceLogRepository>();
            For<ICustomerMarketPlaceRepository>().Use<CustomerMarketPlaceRepository>();
            For<IMP_WhiteListRepository>().Use<MP_WhiteListRepository>();
            For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
            For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
            For<ILoanTransactionRepository>().Use<LoanTransactionRepository>();
            For<ILoanAgreementRepository>().Use<LoanAgreementRepository>();
            For<ILoanRepository>().Use<LoanRepository>();
            For<IAgreementsGenerator>().Use<AgreementsGenerator>();
            For<ILoanOptionsRepository>().Use<LoanOptionsRepository>();

            if (bobconfig.PayPoint.ValidateName)
            {
                For<ICustomerNameValidator>().Use<CustomerNameValidator>();
            }
            else
            {
                For<ICustomerNameValidator>().Use<FakeCustomerNameValidator>();
            }

            if (ezBobConfiguration.CheckStoreUniqueness)
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
            For<IPostcodeAnywhereConfig>().Use(ezBobConfiguration.PostcodeAnywhereConfig);

            if (ezBobConfiguration.PostcodeAnywhereConfig.Enabled)
            {
                For<ISortCodeChecker>().Use<SortCodeChecker>();
            }
            else
            {
                For<ISortCodeChecker>().Use<FakeSortCodeChecker>();
            }

            For<IZohoConfig>().Use(bobconfig.ZohoCRM);
            if (bobconfig.ZohoCRM.Enabled)
            {
                /*For<IZohoFacade>().Use<ZohoFacade>();*/
                /*For<IZohoFacade>().Use<ZohoFacadeSignaled>();*/
                For<IZohoFacade>().Use<EzBob.Web.Code.ZohoFacadeSignaled>();
            }
            else
            {
                For<IZohoFacade>().Use<ZohoFacadeFake>();
            }

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
            For<IPostCodeFacade>().Use<SimplyPostCodeFacade>();
            For<IDiscountPlanRepository>().Use <DiscountPlanRepository>();
            For<ICurrencyConvertor>().Use<CurrencyConvertor>();
            For<IMandrillConfig>().Use(bobconfig.MandrillConfig);
            For<IMail>().Use<Mail>();
        }
    }
}