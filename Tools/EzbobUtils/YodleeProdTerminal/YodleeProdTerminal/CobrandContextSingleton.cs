using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;
using System.Web.Services.Protocols;
using System.Reflection;

namespace com.yodlee.sampleapps
{
    public sealed class CobrandContextSingleton
    {
        static CobrandContextSingleton instance = null;
        static readonly object padlock = new object();
        static CobrandContext cobrandContext = null;       
        double COBRAND_CONTEXT_TIME_OUT = 3;
        DateTime created = DateTime.Now;
        CobrandLoginService cobrandLoginService;

        CobrandContextSingleton()
        {
            created = created.AddMinutes(-COBRAND_CONTEXT_TIME_OUT);
            string soapServer = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer");
            System.Console.WriteLine("Connection to soapServer " + soapServer + "...");
            System.Environment.SetEnvironmentVariable("com.yodlee.soap.services.url", soapServer);
            cobrandLoginService = new CobrandLoginService();
            cobrandLoginService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + cobrandLoginService.GetType().FullName;
        }

        public static CobrandContextSingleton Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance==null)
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
            DateTime expired = created.AddMinutes(COBRAND_CONTEXT_TIME_OUT );

            if (now >= expired)
            {
               // System.Console.WriteLine("\t(CobrandContext is old, creating new one...)");

                // Cobrand Context expired, create new one
                cobrandLoginService = new CobrandLoginService();
                cobrandLoginService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + cobrandLoginService.GetType().FullName;
                // Get Cobrand Credentials from AppSettings (requires App.config file)
                string cobrandIdStr = System.Configuration.ConfigurationManager.AppSettings.Get("cobrandId");
                long cobrandId = long.Parse(cobrandIdStr);
                string applicationId = System.Configuration.ConfigurationManager.AppSettings.Get("applicationId");
                string username = System.Configuration.ConfigurationManager.AppSettings.Get("username");
                string password = System.Configuration.ConfigurationManager.AppSettings.Get("password");
                string tncVersionStr = System.Configuration.ConfigurationManager.AppSettings.Get("tncVersion");
                long tncVersion = long.Parse(tncVersionStr);
                // Note you can remove warnings by adding reference 'System.Configuration' from the .NET tab
                // and replacing code "ConfigurationManager.AppSettings.Get" with "ConfigurationManager.AppSettings"
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
               // System.Console.WriteLine("\t(using cached CobrandContext...)");
                return cobrandContext;
            }
        }
    }
}
