namespace EzBobRest.NancySwagger.Fluent
{
    using System;
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Model;

    public static class SwaggerRouteMetadataExtensions
    {
        public static SwaggerRouteMetadata With(this SwaggerRouteMetadata routeMetadata,
            Func<SwaggerEndpointInfo, SwaggerEndpointInfo> info)
        {
            routeMetadata.Info = info(routeMetadata.Info ?? new SwaggerEndpointInfo());

            return routeMetadata;
        }
    }
}
