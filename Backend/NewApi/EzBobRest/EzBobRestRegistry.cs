namespace EzBobRest {
    using EzBobCommon.Injection;
    using EzBobCommon.NSB;
    using EzBobRest.Init;
    using Microsoft.Owin.Security.OAuth;

    /// <summary>
    /// Registers dependencies
    /// </summary>
    public class EzBobRestRegistry : EzRegistryBase {
        public EzBobRestRegistry() {
            RegisterRestServer();
        }

        /// <summary>
        /// Registers the rest server.
        /// </summary>
        private void RegisterRestServer() {
            For<IStartup>()
                .Use<Startup>();

            For<OAuthAuthorizationServerProvider>()
                .Use<EzBobOAuth2AuthorizationProvider>();
        }
    }
}
