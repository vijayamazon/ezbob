using System.Web.Security;
using ApplicationMng.Model.Commands.Player;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Email;
using EZBob.DatabaseLib.Model.Loans;
using EZBob.DatabaseLib.PyaPalDetails;
using EZBob.DatabaseLib.Repository;
using EzBob.AmazonServiceLib.Config;
using EzBob.Configuration;
using EzBob.PayPalServiceLib;
using EzBob.PayPalServiceLib.Common;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Controllers;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Code.ApplicationCreator;
using EzBob.Web.Code.Email;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Code.PostCode;
using EzBob.Web.Models.Repository;
using EzBob.eBayLib.Config;
using PayPal.Platform.SDK;
using PaymentServices.Calculators;
using PostcodeAnywhere;
using Scorto.Configuration;
using Scorto.Web;
using StructureMap.Configuration.DSL;
using ZohoCRM;

namespace EzBob.Web.Infrastructure
{
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
            For<IServiceEndPointFactory>().Use(new ServiceEndPointFactory());
            For<IDbStringRepository>().Use<DbStringRepository>();
            For<EzBobConfigRoot>().Use(c => localRoot);
            For<IPayPointFacade>().Use<PayPointFacade>();
            For<IPersonalInfoHistoryRepository>().Use<PersonalInfoHistoryRepository>();
            For<IPacnetPaypointServiceLogRepository>().Use<PacnetPaypointServiceLogRepository>();
            For<ICustomerMarketPlaceRepository>().Use<CustomerMarketPlaceRepository>();
            For<IPayPalDetailsRepository>().Use<PayPalDetailsRepository>();
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
                For<IZohoFacade>().Use<ZohoFacadeSignaled>();
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
            For<IPostCodeFacade>().Use<SimplyPostCodeFacade>();
            For<IDiscountPlanRepository>().Use <DiscountPlanRepository>();
        }
    }
}