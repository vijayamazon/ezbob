

namespace YodleeLib
{
    using System;
    using StructureMap;
    using YodleeLib.config;

    public sealed class CobrandContextSingleton
    {
        static CobrandContextSingleton instance = null;
        static readonly object padlock = new object();
        static CobrandContext cobrandContext = null;
        double COBRAND_CONTEXT_TIME_OUT = 3;
        DateTime created = DateTime.Now;
        CobrandLoginService cobrandLoginService;
        private static IYodleeMarketPlaceConfig _config;

        CobrandContextSingleton()
        {
            _config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
            created = created.AddMinutes(-COBRAND_CONTEXT_TIME_OUT);
            string soapServer = _config.soapServer;
            Environment.SetEnvironmentVariable("com.yodlee.soap.services.url", soapServer);
            cobrandLoginService = new CobrandLoginService();
            cobrandLoginService.Url = soapServer + "/" + cobrandLoginService.GetType().FullName;
        }

        public static CobrandContextSingleton Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CobrandContextSingleton();
                    }
                    return instance;
                }
            }
        }

        public CobrandContext getCobrandContext()
        {
            DateTime now = DateTime.Now;
            DateTime expired = created.AddMinutes(COBRAND_CONTEXT_TIME_OUT);


            if (now >= expired)
            {
                // Cobrand Context expired, create new one
                cobrandLoginService = new CobrandLoginService();
                cobrandLoginService.Url = _config.soapServer + "/" + cobrandLoginService.GetType().FullName;
                // Get Cobrand Credentials from AppSettings (requires App.config file)
                string cobrandIdStr = _config.cobrandId;
                long cobrandId = long.Parse(cobrandIdStr);
                string applicationId = _config.applicationId;
                string username = _config.username;
                string password = _config.password;
                string tncVersionStr = _config.tncVersion;
                long tncVersion = long.Parse(tncVersionStr);
                // Note you can remove warnings by adding reference 'System.Configuration' from the .NET tab
                // and replacing code "ConfigurationSettings.AppSettings.Get" with "ConfigurationManager.AppSettings"
                // This only works with .NET 2.0 or above.  Leaving code as is for now.s

                Locale locale = new Locale();
                locale.country = "US";
                CobrandPasswordCredentials cobrandPasswordCredentials =
                    new CobrandPasswordCredentials();
                cobrandPasswordCredentials.password = password;
                cobrandPasswordCredentials.loginName = username;


                // authentication of a cobrand in the Yodlee software platform and returns 
                // a valid CobrandContext if the authentication is successful. This method takes a generic CobrandCredentials argument as the
                // authentication related credentials of the cobrand.
                cobrandContext = cobrandLoginService.loginCobrand(
                        cobrandId,
                        true,
                        applicationId,
                        locale,
                        tncVersion,
                        true,
                        cobrandPasswordCredentials);

                created = DateTime.Now;
                return cobrandContext;
            }
            else
            {
                return cobrandContext;
            }
        }
    }
}
