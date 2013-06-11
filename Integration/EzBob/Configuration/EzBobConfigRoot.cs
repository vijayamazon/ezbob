namespace EzBob.Configuration
{
	using FreeAgent.Config;
	using YodleeLib.config;
	using AmazonServiceLib.Config;
	using PayPalServiceLib;
	using eBayLib.Config;
	using Scorto.Configuration;
    using AmazonEnvConnectionConfig = AmazonServiceLib.Config.AmazonEnvConnectionConfig;

    public class EzBobConfigRoot : ConfigurationRoot
    {
        public IPayPalConfig PayPalConfig
        {
            get { return GetConfiguration<PayPalEnvConfig>("PayPalConfig"); }
        }

        public IEbayMarketplaceTypeConnection eBayConfig
        {
            get { return GetConfiguration<EbayEvnConnection>("EbayConfig"); }
        }

        public IEbayMarketplaceSettings eBaySettings
        {
            get { return GetConfiguration<EbayEvnSettings>("EbaySettings"); }
        }

        public IAmazonMarketPlaceTypeConnection AmazonConfig
        {
            get { return GetConfiguration<AmazonEnvConnectionConfig>("AmazonConfig"); }
        }

        public IPayPalMarketplaceSettings PayPalSettings
        {
            get { return GetConfiguration<PayPalEvnSettings>("PayPalSettings"); }
        }

        public IAmazonMarketplaceSettings AmazonSetings
        {
            get { return GetConfiguration<AmazonEvnSettings>("AmazonSetings"); }
        }

		public IYodleeMarketPlaceConfig YodleeConfig
		{
			get { return GetConfiguration<YodleeEnvConnectionConfig>("YodleeConfig"); }
		}

		public IFreeAgentConfig FreeAgentConfig
		{
			get { return GetConfiguration<FreeAgentEnvConnectionConfig>("FreeAgentConfig"); }
		}
    }
}