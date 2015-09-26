namespace AutomationAPI
{
    using System.Web.Http;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
            name: "AutomationApi",
            routeTemplate: "api/{controller}/{action}/{id}",
            defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
            name: "QuickTextReplaceAllApi",
            routeTemplate: "api/{controller}/{action}/{fromStr}/{toStr}",
            defaults: new {fromStr = string.Empty, toStr = string.Empty }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
