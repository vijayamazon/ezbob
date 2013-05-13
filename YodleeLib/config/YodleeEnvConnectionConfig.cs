using Scorto.Configuration;

namespace YodleeLib.config
{
	public class YodleeEnvConnectionConfig : ConfigurationRoot, IYodleeMarketPlaceConfig
    {
        public string cobrandId
        {
            get { return GetValue<string>("cobrandId"); }
        }

        public string applicationId
        {
            get { return GetValue<string>("applicationId"); }
        }

        public string username
        {
            get { return GetValue<string>("username"); }
        }

        public string password
        {
            get { return GetValue<string>("password"); }
        }

        public string soapServer
        {
            get { return GetValue<string>("soapServer"); }
        }

        public string tncVersion
        {
            get { return GetValue<string>("tncVersion"); }
        }

        public string BridgetApplicationID
        {
            get { return GetValue<string>("BridgetApplicationID"); }
        }

        public string ApplicationKey
        {
            get { return GetValue<string>("ApplicationKey"); }
        }

        public string ApplicationToken
        {
            get { return GetValue<string>("ApplicationToken"); }
        }

        public string AddAccountURL
        {
            get { return GetValue<string>("AddAccountURL"); }
        }

        public string EditAccountURL
        {
            get { return GetValue<string>("EditAccountURL"); }
        }
    }
}