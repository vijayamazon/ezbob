using System.Runtime.CompilerServices;
using Scorto.Configuration;

namespace EzBob.Configuration
{
    public class ConfigurationRootBob : ConfigurationBase
    {
        private static ConfigurationRootBob _configuration;

        public virtual PacNetConfiguration PacNet
        {
            get { return GetConfiguration<PacNetConfiguration>("PacNet"); }
        }

        public virtual PayPointConfiguration PayPoint
        {
            get { return GetConfiguration<PayPointConfiguration>("PayPoint"); }
        }

        public virtual IZohoConfig ZohoCRM
        {
            get { return GetConfiguration<ZohoConfig>("ZohoCRM"); }
        }

        public virtual ExperianIntegrationParams Experian
        {
            get { return GetConfiguration<ExperianIntegrationParams>("Experian"); }
        }

        public virtual IMandrillConfig MandrillConfig
        {
            get { return GetConfiguration<MandrillConfig>("MandrillConfig"); }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ConfigurationRootBob GetConfiguration()
        {
            return _configuration ??
                   (_configuration =
                    EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRootBob>());
        }
    }
}