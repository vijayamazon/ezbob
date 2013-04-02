using System;
using EzBob.Configuration;
using Scorto.Configuration;

namespace Raven.API.Support
{
    public class XMLConfiguration
    {
        readonly PacNetConfiguration _configParams;

        public XMLConfiguration(String cfgFilename, String nodeTagName)
        {
            _configParams = ConfigurationRootBob.GetConfiguration().PacNet;
        }

        public String GetString(String paramKey)
        {
            try
            {
                return _configParams.GetValue<string>(paramKey);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public String GetString(String paramKey, String defaultValue)
        {
            try
            {
                return _configParams.GetValueWithDefault<string>(paramKey, defaultValue);
            }
            catch (Exception)
            {
                return null;
            }
            
        }
    }
}
