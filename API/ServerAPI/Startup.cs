using Ezbob.API.ServerAPI;
//using Microsoft.Owin;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]

namespace Ezbob.API.ServerAPI {
	using System;
	using System.Net.Http.Formatting;
	using System.Web.Http;
	using Microsoft.Owin;
	using Microsoft.Owin.Cors;
	using Microsoft.Owin.Security.OAuth;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;
	using Owin;

	public class Startup {


		public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

		public void Configuration(IAppBuilder app) {

			HttpConfiguration config = new HttpConfiguration();

			
			ConfigureOAuth(app);

			WebApiConfig.Register(config);

			app.UseCors(CorsOptions.AllowAll);

			app.UseWebApi(config);
		}

		

		//add  AllowInsecureHttp = false to all
		private void ConfigureOAuth(IAppBuilder app) {

			OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

			//Token Consumption
			app.UseOAuthBearerAuthentication(OAuthBearerOptions);
		}
	}

	
}