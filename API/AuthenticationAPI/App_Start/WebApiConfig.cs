namespace Ezbob.API.AuthenticationAPI {
	using System.Linq;
	using System.Net.Http.Formatting;
	using System.Web.Http;
	using Microsoft.Owin.Security.OAuth;
	using Newtonsoft.Json.Serialization;
	using WebApi.OutputCache.Core.Cache;
	using WebApi.OutputCache.V2;


	public static class WebApiConfig {

		public static void Register(HttpConfiguration config) {
			// Web API routes
			config.MapHttpAttributeRoutes();

			// Configure Web API to use only bearer token authentication.
			config.SuppressDefaultHostAuthentication();
			config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			config.Filters.Add(new AuthorizeAttribute());

			/*	var eTagStore = new SqlServerEntityTagStore(new CurrentDbConnection().ConnectionStringName);
				var cacheHandler = new CachingHandler(eTagStore);
				config.MessageHandlers.Add(cacheHandler);*/

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			config.CacheOutputConfiguration().RegisterCacheOutputProvider(() => new MemoryCacheDefault());

			var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
			jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
		}
	}
}