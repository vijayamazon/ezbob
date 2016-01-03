namespace EzBobRest.Modules
{
    using EzBobRest.NancySwagger.Modules;
    using Nancy.Routing;

    public class DocsModule : SwaggerDocsModuleBase
    {
        public DocsModule(IRouteCacheProvider routeCacheProvider)
            : base(routeCacheProvider,
              "Ezbob API v.1 documentation",   // title
              "v1.0",                       // api version
              "localhost:12345",             // host
              "/api/v1",                       // api base url (ie /dev, /api)
              "http")                       // schemes
        {
        }
    }
}
