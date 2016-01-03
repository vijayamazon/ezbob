﻿namespace EzBobRest.NancySwagger.Fluent
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using EzBobRest.NancySwagger.Model;
    using Nancy;
    using NJsonSchema;

    public static class SwaggerEndpointInfoExtensions
    {
        public static SwaggerEndpointInfo WithResponseModel(this SwaggerEndpointInfo endpointInfo, HttpStatusCode statusCode, Type modelType, string description = null)
        {
            if (endpointInfo.ResponseInfos == null)
            {
                endpointInfo.ResponseInfos = new Dictionary<string, SwaggerResponseInfo>();
            }

            endpointInfo.ResponseInfos[StatusToString(statusCode)] = GenerateResponseInfo(description, modelType);

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithDefaultResponse(this SwaggerEndpointInfo endpointInfo, Type responseType)
        {
            return endpointInfo.WithResponseModel(HttpStatusCode.OK, responseType);
        }

        public static SwaggerEndpointInfo WithResponse(this SwaggerEndpointInfo endpointInfo, HttpStatusCode statusCode, string description)
        {
            if (endpointInfo.ResponseInfos == null)
            {
                endpointInfo.ResponseInfos = new Dictionary<string, SwaggerResponseInfo>();
            }

            endpointInfo.ResponseInfos[StatusToString(statusCode)] = GenerateResponseInfo(description);

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithRequestParameter(this SwaggerEndpointInfo endpointInfo, string name,
            string description = null, string type = "string", string format = null, bool required = true, string loc = "path")
        {
            if (endpointInfo.RequestParameters == null)
            {
                endpointInfo.RequestParameters = new List<SwaggerRequestParameter>();
            }

            endpointInfo.RequestParameters.Add(new SwaggerRequestParameter
            {
                Required = required,
                Description = description,
                Format = format,
                In = loc,
                Name = name,
                Type = type
            });

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithRequestModel(this SwaggerEndpointInfo endpointInfo, Type requestType, string name = "body", string description = null, bool required = true, string loc = "body")
        {
            if (endpointInfo.RequestParameters == null)
            {
                endpointInfo.RequestParameters = new List<SwaggerRequestParameter>();
            }

            endpointInfo.RequestParameters.Add(new SwaggerRequestParameter
            {
                Required = required,
                Description = description,
                In = loc,
                Name = name,
                Schema = GetSchema(requestType)
            });

            return endpointInfo;
        }

        public static SwaggerEndpointInfo WithDescription(this SwaggerEndpointInfo endpointInfo, string description, params string[] tags)
        {
            if (endpointInfo.Tags == null)
            {
                if (tags.Length == 0)
                {
                    tags = new[] {"default"};
                }

                endpointInfo.Tags = tags;
            }

            endpointInfo.Description = description;

            return endpointInfo;
        }

        private static SwaggerResponseInfo GenerateResponseInfo(string description, Type responseType)
        {
            return new SwaggerResponseInfo
            {
                Schema = GetSchema(responseType),
                Description = description
            };
        }

        private static SwaggerResponseInfo GenerateResponseInfo(string description)
        {
            return new SwaggerResponseInfo
            {
                Description = description
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string StatusToString(HttpStatusCode statusCode) {
            return ((int)statusCode).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static JsonSchema4 GetSchema(Type type) {
            return JsonSchema4.FromType(type, new JsonSchemaGeneratorSettings{DefaultEnumHandling = EnumHandling.String});

//            JSchemaGenerator generator = new JSchemaGenerator();
//
//            JSchema schema =  generator.Generate(type);
//
//            // I didn't find the way how to disallow JSchemaGenerator to use nullable types, swagger doesn't work with them
//
//            string tmp = schema.ToString();
//            string s = @"\""type\"":[\s\n\r]*\[[\s\n\r]*\""(\w+)\"",[\s\n\r]*\""null\""[\s\n\r]*\]";
//            tmp = Regex.Replace(tmp, s, "\"type\": \"$1\"");
//
//            return JSchema.Parse(tmp);
        }
    }
}