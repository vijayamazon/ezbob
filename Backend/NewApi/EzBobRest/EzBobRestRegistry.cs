namespace EzBobRest {
    using System.IO;
    using System.Text;
    using Common.Logging;
    using EzBobCommon.Configuration;
    using EzBobCommon.Injection;
    using EzBobCommon.NSB;
    using EzBobRest.Init;
    using EzBobRest.Properties;
    using Microsoft.Owin.Security.OAuth;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    /// <summary>
    /// Registers dependencies
    /// </summary>
    public class EzBobRestRegistry : EzRegistryBase {
        public EzBobRestRegistry() {

            ForSingletonOf<SendReceiveCache>()
                .Use<SendReceiveCache>();

            ForSingletonOf<IHandlersProvider>()
                .Use<HandlersProvider>();

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
