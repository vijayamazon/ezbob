namespace EzBob.Configuration
{
	using PayPalServiceLib;
	using Scorto.Configuration;

    public class EzBobConfigRoot : ConfigurationRoot
    {
        public IPayPalConfig PayPalConfig
        {
            get { return GetConfiguration<PayPalEnvConfig>("PayPalConfig"); }
        }
    }
}