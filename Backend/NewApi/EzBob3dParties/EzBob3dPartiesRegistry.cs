namespace EzBob3dParties {
    using EzBob3dParties.EBay;
    using EzBob3dParties.HMRC;
    using EzBob3dParties.Yodlee;
    using EzBobCommon.Injection;
    using EzBobCommon.NSB;
    using EzBobCommon.Web;

    /// <summary>
    /// Configures DI container
    /// </summary>
    public class EzBob3dPartiesRegistry : EzRegistryBase {
        public EzBob3dPartiesRegistry() {

            ForSingletonOf<IEzBobHtmlClient>()
                .Use<EzBobHtmlClient>();

            For<IEzBobWebBrowser>()
                .Use<EzBobWebBrowser>();

            ForSingletonOf<IHandlersProvider>()
                .Use<HandlersProvider>();

            InitYodleeRelated();

            InitHMRCRelated();

            InitEbayRelated();
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
    }
}
