namespace EzBobService {
    using EzBobCommon.Injection;
    using EzBobPersistence;
    using EzBobPersistence.Alibaba;
    using EzBobPersistence.Broker;
    using EzBobPersistence.Company;
    using EzBobPersistence.Currency;
    using EzBobPersistence.Customer;
    using EzBobPersistence.DocsUpload;
    using EzBobPersistence.Loan;
    using EzBobPersistence.MarketPlace;
    using EzBobPersistence.MobilePhone;
    using EzBobPersistence.ThirdParty.Amazon;
    using EzBobPersistence.ThirdParty.Experian;
    using EzBobPersistence.ThirdParty.Hrmc;
    using EzBobService.Currency;
    using EzBobService.Customer;
    using EzBobService.Misc;
    using EzBobService.Mobile;
    using EzBobService.ThirdParties.Hmrc.Upload;

    public class EzBobServiceRegistry : EzRegistryBase {
        //TODO: read from configuration
        private static readonly string TheConnectionString = "Server=localhost;Database=ezbob;User Id=ezbobuser;Password=ezbobuser;MultipleActiveResultSets=true";
        private static readonly string ConnectionString = "connectionString";

        public EzBobServiceRegistry() {
//            Scan(scan => {
//                scan.TheCallingAssembly();
//                scan.WithDefaultConventions();
//                scan.With(new CommandHandlerConvention());
//            });

           For<IHmrcQueries>()
                .Use<HmrcQueries>();

            For<IHmrcUploadedFileParser>()
                .Use<HmrcUploadedFileParser>();

            ForSingletonOf<MobilePhone>()
                .Use<MobilePhone>();
            ForSingletonOf<IMobilePhoneQueries>()
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

            ForSingletonOf<IBrokerQueries>()
                .Use<BrokerQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<IDocsUploadQueries>()
                .Use<DocsUploadQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<IMarketPlaceQueries>()
                .Use<MarketPlaceQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<IAmazonOrdersQueries>()
                .Use<AmazonOrdersQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);

            ForSingletonOf<IAmazonCategoriesQueries>()
                .Use<AmazonCategoriesQueries>()
                .Ctor<string>(ConnectionString)
                .Is(TheConnectionString);


            ForSingletonOf<CustomerProcessor>()
                .Use<CustomerProcessor>();

            ForSingletonOf<RefNumberGenerator>()
                .Use<RefNumberGenerator>();

            ForSingletonOf<ICurrencyConverter>()
                .Use<CurrencyConverter>();
        }
    }
}
