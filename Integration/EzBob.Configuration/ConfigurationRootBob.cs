﻿using System.Runtime.CompilerServices;
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

		public virtual YodleeEnvConnectionConfig YodleeConfig
		{
			get { return GetConfiguration<YodleeEnvConnectionConfig>("YodleeConfig"); }
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