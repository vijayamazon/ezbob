

using Ezbob.API.AuthenticationAPI;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]

namespace Ezbob.API.AuthenticationAPI
{
	using System;
	using System.Web.Http;
	using Ezbob.API.AuthenticationAPI.Providers;
	using Microsoft.AspNet.Identity;
	using Microsoft.Owin;
	using Microsoft.Owin.Cors;
	using Microsoft.Owin.Security.Facebook;
	using Microsoft.Owin.Security.Google;
	using Microsoft.Owin.Security.OAuth;
	using Owin;

	public class StartupCopy {
		public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
		public static GoogleOAuth2AuthenticationOptions googleAuthOptions { get; private set; }
		public static FacebookAuthenticationOptions facebookAuthOptions { get; private set; }

		public void Configuration(IAppBuilder app) {
			HttpConfiguration config = new HttpConfiguration();

			ConfigureOAuth(app);

			WebApiConfig.Register(config);
			app.UseCors(CorsOptions.AllowAll);
			app.UseWebApi(config);

			//Database.SetInitializer(new MigrateDatabaseToLatestVersion<AuthContext, Configuration>());
		}

		public void ConfigureOAuth(IAppBuilder app) {
			//use a cookie to temporarily store information about a user logging in with a third party login provider
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
			OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

			OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions() {

				AllowInsecureHttp = false,
				TokenEndpointPath = new PathString("/token"),
				AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
				Provider = new SimpleAuthorizationServerProvider(),
				RefreshTokenProvider = new SimpleRefreshTokenProvider()
			};

			// Token Generation
			app.UseOAuthAuthorizationServer(OAuthServerOptions);
			app.UseOAuthBearerAuthentication(OAuthBearerOptions);

			//Configure Google External Login
			googleAuthOptions = new GoogleOAuth2AuthenticationOptions() {
				ClientId = "xxxxxx",
				ClientSecret = "xxxxxx",
				Provider = new GoogleAuthProvider()
			};
			app.UseGoogleAuthentication(googleAuthOptions);

			//Configure Facebook External Login
			facebookAuthOptions = new FacebookAuthenticationOptions() {
				AppId = "xxxxxx",
				AppSecret = "xxxxxx",
				Provider = new FacebookAuthProvider()
			};
			app.UseFacebookAuthentication(facebookAuthOptions);

		}
	}

}