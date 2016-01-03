namespace EzBobService {
    using System.IO;
    using System.Text;
    using EzBobCommon.Configuration;
    using EzBobCommon.Currencies;
    using EzBobCommon.NSB;
    using EzBobPersistence;
    using EzBobPersistence.Alibaba;
    using EzBobPersistence.Company;
    using EzBobPersistence.Currency;
    using EzBobPersistence.Customer;
    using EzBobPersistence.Loan;
    using EzBobPersistence.MobilePhone;
    using EzBobPersistence.ThirdParty.Experian;
    using EzBobService.Currency;
    using EzBobService.Customer;
    using EzBobService.DependencyResolution;
    using EzBobService.Misc;
    using EzBobService.Mobile;
    using EzBobService.Properties;
    using log4net;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    public class EzBobServiceRegistry : Registry {
        //TODO: read from configuration
        private static readonly string TheConnectionString = "Server=localhost;Database=ezbob;User Id=ezbobuser;Password=ezbobuser;MultipleActiveResultSets=true";
        private static readonly string ConnectionString = "connectionString";

        public EzBobServiceRegistry() {
            Scan(scan => {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
                scan.With(new CommandHandlerConvention());
            });

            For<ILog>()
                .Add(ctx => LogManager.GetLogger(ctx.ParentType));

            ForSingletonOf<ConfigManager>()
                .Use<ConfigManager>()
                .OnCreation(cnfg => InitConfigurationManger(cnfg));

            //handles configuration objects injection
            Policies.OnMissingFamily<ConfigurationPolicy>();

            ForSingletonOf<MobilePhone>()
                .Use<MobilePhone>();
            ForSingletonOf<MobilePhoneQueries>()
                .Use<MobilePhoneQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<ICompanyQueries>()
                .Use<CompanyQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<ICustomerQueries>()
                .Use<CustomerQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);
            ForSingletonOf<ExperianQuery>()
                .Use<ExperianQuery>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);
            ForSingletonOf<ILoanQueries>()
                .Use<LoanQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);
            ForSingletonOf<ConfigurationQueries>()
                .Use<ConfigurationQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);
            ForSingletonOf<IAlibabaQueries>()
                .Use<AlibabaQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<ICurrencyQueries>()
                .Use<CurrencyQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<CustomerProcessor>()
                .Use<CustomerProcessor>();

            ForSingletonOf<IHandlersProvider>()
                .Use<HandlersProvider>();

            ForSingletonOf<ErrorCache>()
                .Use<ErrorCache>();

            ForSingletonOf<RefNumberGenerator>()
                .Use<RefNumberGenerator>();

            ForSingletonOf<ICurrencyConverter>()
                .Use<CurrencyConverter>();

        }

        private void InitConfigurationManger(ConfigManager configManager) {
            configManager.AddConfigJsonString(Encoding.UTF8.GetString(Resources.config));
            if (File.Exists("config.json")) {
                configManager.AddConfigFilePaths("config.json");
            }
        }
    }
}
