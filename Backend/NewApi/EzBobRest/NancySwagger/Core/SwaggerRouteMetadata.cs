namespace EzBobRest.NancySwagger.Core
{
    using EzBobRest.NancySwagger.Model;
    using Nancy.Routing;

    public class SwaggerRouteMetadata
    {
        public SwaggerRouteMetadata(string path, string method)
        {
            Path = path;
            Method = method.ToLower();
        }

        public SwaggerRouteMetadata(RouteDescription desc) : this(desc.Path, desc.Method) { }

        public string Path { get; set; }

        public string Method { get; set; }

        public SwaggerEndpointInfo Info { get; set; }
    }
}