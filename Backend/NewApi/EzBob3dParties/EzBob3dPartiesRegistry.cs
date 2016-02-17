namespace EzBob3dParties {
    using EzBob3dParties.Amazon;
    using EzBob3dParties.Amazon.RatingScraper;
    using EzBob3dParties.Amazon.Src.CustomerApi;
    using EzBob3dParties.Amazon.Src.OrdersApi;
    using EzBob3dParties.Amazon.Src.ProductsApi;
    using EzBob3dParties.EBay;
    using EzBob3dParties.Hmrc;
    using EzBob3dParties.PayPalService.Soap;
    using EzBob3dParties.SimplyPostcode;
    using EzBob3dParties.Twilio;
    using EzBob3dParties.Yodlee;
    using EzBobCommon.Injection;
    using EzBobCommon.Web;

    /// <summary>
    /// Configures DI container
    /// </summary>
    public class EzBob3dPartiesRegistry : EzRegistryBase {
        public EzBob3dPartiesRegistry() {

            ForSingletonOf<IEzBobHttpClient>()
                .Use<EzBobHttpClient>();

            For<IEzBobWebBrowser>()
                .Use<EzBobWebBrowser>();

            For<ITwilio>()
                .Use<TwilioService>();

            For<PayPalSoapService>()
                .Use<PayPalSoapService>();

            For<ISimplyPostcodeService>()
                .Use<SimplyPostcodeService>();

            InitYodleeRelated();

            InitHMRCRelated();

            InitEbayRelated();

            InitAmazonRelated();
        }

        /// <summary>
        /// Initializes the ebay related.
        /// </summary>
        private void InitEbayRelated() {
            ForSingletonOf<EBayService>()
                .Use<EBayService>();
        }

        /// <summary>
        /// Initializes the yodlee related.
        /// </summary>
        private void InitYodleeRelated() {
            For<YodleeService>()
                .Use<YodleeService>();
        }

        /// <summary>
        /// Initializes the HMRC related.
        /// </summary>
        private void InitHMRCRelated() {

            ForSingletonOf<UserVatIdAndTaxOfficeNumberFetcher>()
                .Use<UserVatIdAndTaxOfficeNumberFetcher>();

            ForSingletonOf<LoginDetailsScraper>()
                .Use<LoginDetailsScraper>();

            For<VatReturnsInfoFetcher>()
                .Use<VatReturnsInfoFetcher>();

            For<UserVatIdAndTaxOfficeNumberFetcher>()
                .Use<UserVatIdAndTaxOfficeNumberFetcher>();

            For<VatReturnInfoParser>()
                .Use<VatReturnInfoParser>();

            ForSingletonOf<GBPParser>()
                .Use<GBPParser>();

            ForSingletonOf<RtiTaxYearParser>()
                .Use<RtiTaxYearParser>();

            For<RtiTaxYearsFetcher>()
                .Use<RtiTaxYearsFetcher>();
        }

        /// <summary>
        /// Initializes the amazon related.
        /// </summary>
        private void InitAmazonRelated() {

            For<IAmazonService>()
                .Use<AmazonService>();

            For<IAmazonCustomerRating>()
                .Use<AmazonCustomerRatingScraper>();

            For<IMwsCustomerService>()
                .Use<MWSCustomerServiceClient>()
                .SelectConstructor(() => new MWSCustomerServiceClient());

            For<IMwsOrdersService>()
                .Use<ImwssOrdersServiceClient>()
                .SelectConstructor(() => new ImwssOrdersServiceClient());

            For<IMwsProductsService>()
                .Use<ImwsProductsServiceClient>()
                .SelectConstructor(() => new ImwsProductsServiceClient());
        }
    }
}
