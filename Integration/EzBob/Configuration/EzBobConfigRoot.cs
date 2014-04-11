namespace EzBob.Configuration
{
	using AmazonServiceLib.Config;
	using PayPalServiceLib;
	using Scorto.Configuration;
    using AmazonEnvConnectionConfig = AmazonServiceLib.Config.AmazonEnvConnectionConfig;

    public class EzBobConfigRoot : ConfigurationRoot
    {
        public IPayPalConfig PayPalConfig
        {
            get { return GetConfiguration<PayPalEnvConfig>("PayPalConfig"); }
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
    }
}