namespace UIAutomationTests 
{
    using System;
    using TestStack.Seleno.Configuration;
    using TestStack.Seleno.Configuration.WebServers;

    public static class BrowserHost {

        public static readonly SelenoHost instance;
        
        static BrowserHost() {
                instance = new SelenoHost();
                instance.Run(configure => configure
                .WithWebServer(new InternetWebServer("https://localhost:44300/Customer/Wizard")));
        }



    }
}
