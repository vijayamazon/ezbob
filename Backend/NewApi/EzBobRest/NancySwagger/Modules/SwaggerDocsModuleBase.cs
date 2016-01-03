namespace EzBobRest.NancySwagger.Modules
{
    using System;
    using System.Collections.Generic;
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Model;
    using Nancy;
    using Nancy.Routing;
    using Newtonsoft.Json;

    public abstract class SwaggerDocsModuleBase : NancyModule
    {
        private SwaggerSpecification swaggerSpecification;

        private readonly IRouteCacheProvider routeCacheProvider;
        private readonly string title;
        private readonly string apiVersion;
        private readonly string host;
        private readonly string apiBaseUrl;
        private readonly string[] schemes;

        protected SwaggerDocsModuleBase(IRouteCacheProvider routeCacheProvider,
            string title = "API documentation",
            string apiVersion = "1.0",
            string host = "localhost:5000",
            string apiBaseUrl = "/",
            params string[] schemes)
            : base()
        {
            this.routeCacheProvider = routeCacheProvider;
            this.title = title;
            this.apiVersion = apiVersion;
            this.host = host;
            this.apiBaseUrl = apiBaseUrl;
            this.schemes = schemes;

            Get[apiBaseUrl] = r => GetDocumentation();
        }

        public virtual Response GetDocumentation()
        {
            try
            {
                if (this.swaggerSpecification == null)
                {
                    GenerateSpecification();
                }


                return
                    Response.AsText(JsonConvert.SerializeObject(this.swaggerSpecification, Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private void GenerateSpecification()
        {
            this.swaggerSpecification = new SwaggerSpecification
            {
                ApiInfo = new SwaggerApiInfo
                {
                    Title = this.title,
                    Version = this.apiVersion,
                },
                Host = this.host,
                BasePath = this.apiBaseUrl,
                Schemes = this.schemes,
            };

            // generate documentation
            IEnumerable<SwaggerRouteMetadata> metadata = this.routeCacheProvider.GetCache()
                .RetrieveMetadata<SwaggerRouteMetadata>();

            Dictionary<string, Dictionary<string, SwaggerEndpointInfo>> endpoints = new Dictionary<string, Dictionary<string, SwaggerEndpointInfo>>();

            foreach (SwaggerRouteMetadata m in metadata)
            {
                if (m == null)
                {
                    continue;
                }

                string path = m.Path;

                if (!string.IsNullOrEmpty(this.swaggerSpecification.BasePath) && this.swaggerSpecification.BasePath != "/")
                {
                    path = path.Replace(this.swaggerSpecification.BasePath, "");
                }

                if (!endpoints.ContainsKey(path))
                {
                    endpoints[path] = new Dictionary<string, SwaggerEndpointInfo>();
                }

                endpoints[path].Add(m.Method, m.Info);
            }

            this.swaggerSpecification.PathInfos = endpoints;
        }
    }
}