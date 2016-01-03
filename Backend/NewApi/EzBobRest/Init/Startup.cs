namespace EzBobRest.Init {
    using System;
    using EzBobCommon;
    using Microsoft.Owin;
    using Microsoft.Owin.Security.OAuth;
    using Nancy.Bootstrapper;
    using Nancy.Owin;
    using Owin;

    public class Startup : IStartup {
        [Injected]
        public OAuthAuthorizationServerProvider AuthorizationProvider { get; set; }

        [Injected]
        public INancyBootstrapper NancyBootstrapper { get; set; }

        public void Configuration(IAppBuilder app) {
            // token generation
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions {
                // for demo purposes
#if DEBUG
                AllowInsecureHttp = true,
#endif
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(8),
                Provider = AuthorizationProvider
            });

            // token consumption
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            //configures nancy to use specific bootstrapper
            NancyOptions options = new NancyOptions {
                Bootstrapper = NancyBootstrapper
            };

            app.UseNancy(options);
//#if DEBUG
//            app.UseErrorPage();
//#endif
//            app.UseWelcomePage("/");
        }
    }
}
