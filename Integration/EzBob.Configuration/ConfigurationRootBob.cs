namespace EzBob.Configuration
{
	using System.Runtime.CompilerServices;
	using Scorto.Configuration;

    public class ConfigurationRootBob : ConfigurationBase
    {
        private static ConfigurationRootBob _configuration;
		
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ConfigurationRootBob GetConfiguration()
        {
            return _configuration ??
                   (_configuration =
                    EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRootBob>());
        }
    }
}