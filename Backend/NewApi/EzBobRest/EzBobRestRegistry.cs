namespace EzBobRest {
    using System.IO;
    using System.Text;
    using Common.Logging;
    using EzBobCommon.Configuration;
    using EzBobCommon.NSB;
    using EzBobRest.Init;
    using EzBobRest.Properties;
    using Microsoft.Owin.Security.OAuth;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    /// <summary>
    /// Registers dependencies
    /// </summary>
    public class EzBobRestRegistry : Registry {
        public EzBobRestRegistry() {
            Scan(scan => {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            //!!!!
            //There are different classes that you will not see here registered in container
            //The objects of these classes created automatically (for example validators)

            For<ILog>()
                .Add(ctx => LogManager.GetLogger(ctx.ParentType.Name)).AlwaysUnique();

            RegisterConfiguration();

            ForSingletonOf<SendReceiveCache>()
                .Use<SendReceiveCache>();

            ForSingletonOf<IHandlersProvider>()
                .Use<HandlersProvider>();

            RegisterRestServer();
        }

        /// <summary>
        /// Registers the configuration.
        /// </summary>
        private void RegisterConfiguration() {
            ForSingletonOf<ConfigManager>()
                .Use<ConfigManager>()
                .OnCreation(cnfg => InitConfigurationManger(cnfg));

            //handles configuration objects injection
            Policies.OnMissingFamily<ConfigurationPolicy>();
        }

        /// <summary>
        /// Initializes the configuration manger.
        /// </summary>
        /// <param name="configManager">The configuration manager.</param>
        private void InitConfigurationManger(ConfigManager configManager) {
            configManager.AddConfigJsonString(Encoding.UTF8.GetString(Resources.config));
            if (File.Exists("config.json")) {
                configManager.AddConfigFilePaths("config.json");
            }
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
