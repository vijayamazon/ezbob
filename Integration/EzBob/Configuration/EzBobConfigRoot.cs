namespace EzBob.Configuration
{
	using AmazonServiceLib.Config;
	using PayPalServiceLib;
	using Scorto.Configuration;

    public class EzBobConfigRoot : ConfigurationRoot
    {
        public IPayPalConfig PayPalConfig
        {
            get { return GetConfiguration<PayPalEnvConfig>("PayPalConfig"); }
        }

        public IPayPalMarketplaceSettings PayPalSettings
        {
            get { return GetConfiguration<PayPalEvnSettings>("PayPalSettings"); }
        }
    }
}