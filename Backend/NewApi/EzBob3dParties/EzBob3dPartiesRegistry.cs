namespace EzBob3dParties {
    using System.IO;
    using System.Text;
    using Common.Logging;
    using EzBob3dParties.EBay;
    using EzBob3dParties.HMRC;
    using EzBob3dParties.Properties;
    using EzBob3dParties.Yodlee;
    using EzBobCommon.Configuration;
    using EzBobCommon.NSB;
    using EzBobCommon.Web;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    public class EzBob3dPartiesRegistry : Registry {
        public EzBob3dPartiesRegistry() {

            Scan(scan => {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            For<ILog>()
                .Add(ctx => LogManager.GetLogger(ctx.ParentType));

            InitConfiguration();

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

        private void InitEbayRelated() {
            ForSingletonOf<EBayService>()
                .Use<EBayService>();
        }

        private void InitYodleeRelated() {
            For<YodleeService>()
                .Use<YodleeService>()
                .OnCreation(o => o.InitAfterInject()); //TODO: when verson 4 of structure map will be released introduce PostContructAttribute with IInstancePolicy
        }

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

        private void InitConfiguration() {
            ForSingletonOf<ConfigManager>()
                .Use<ConfigManager>()
                .OnCreation(cnfg => InitConfigurationManger(cnfg));

            //handles configuration objects injection
            Policies.OnMissingFamily<ConfigurationPolicy>();
        }


        private void InitConfigurationManger(ConfigManager configManager) {
            configManager.AddConfigJsonString(Encoding.UTF8.GetString(Resources.config));
            if (File.Exists("config.json")) {
                configManager.AddConfigFilePaths("config.json");
            }
        }
    }
}
