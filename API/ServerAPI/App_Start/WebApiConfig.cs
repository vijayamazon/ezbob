namespace Ezbob.API.ServerAPI {
	using System.Linq;
	using System.Net.Http.Formatting;
	using System.Web.Http;
	using Ezbob.API.ServerAPI.Attributes;
	using Microsoft.Owin.Security.OAuth;
	using Newtonsoft.Json.Serialization;

	public static class WebApiConfig {
		public static void Register(HttpConfiguration config) {

			// Configure Web API to use only bearer token authentication.
			config.SuppressDefaultHostAuthentication();
			config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			// add global authorization filter
			//config.Filters.Add(new ClaimsAuthorizeAttribute());

			//	config.Filters.Add(new ValidateModelStateAttribute());

			
			//config.Filters.Add(new AuthorizeAttribute()); // Global level

			config.Filters.Add(new RequireHttpsAttribute());

			// Web API routes
			config.MapHttpAttributeRoutes();

			var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
			jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
		}


		
	}
}