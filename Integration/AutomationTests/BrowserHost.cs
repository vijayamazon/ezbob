namespace UIAutomationTests 
{
    using TestStack.Seleno.Configuration;
    using TestStack.Seleno.Configuration.WebServers;

    public static class BrowserHost {
        
        public static readonly SelenoHost instance = new SelenoHost();

        static BrowserHost() {
            instance.Run(configure => configure
            .WithWebServer(new InternetWebServer("https://localhost:44300/Customer/Wizard")));
        }



    }
}
