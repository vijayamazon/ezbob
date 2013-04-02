using Scorto.Configuration;

namespace EzBob.Configuration
{
    public class ZohoConfig : ConfigurationRoot, IZohoConfig
    {
        public virtual string Token
        {
            get { return GetValue<string>("Token"); }
        }

        public virtual bool Enabled
        {
            get { return GetValueWithDefault<bool>("Enabled", "True"); }
        }
    }
}