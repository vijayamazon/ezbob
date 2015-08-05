using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using TestStack.Seleno.Configuration;

namespace S1.Application.OrangeMoney
{
    using TestStack.Seleno.Configuration.WebServers;

    public static class BrowserHost {
        
        public static readonly SelenoHost instance = new SelenoHost();

        static BrowserHost() {
            instance.Run(configure => configure
            .WithWebServer(new InternetWebServer("https://localhost:44300/Customer/Wizard")));
        }



    }
}
