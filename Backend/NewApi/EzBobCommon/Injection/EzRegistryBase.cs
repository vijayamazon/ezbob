using System;
using System.Linq;

namespace EzBobCommon.Injection {
    using System.IO;
    using System.Reflection;
    using Common.Logging;
    using EzBobCommon.Configuration;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using StructureMap.Configuration.DSL;

    public abstract class EzRegistryBase : Registry {
        protected EzRegistryBase() {


            //handles configuration objects injection
            Policies.OnMissingFamily<ConfigurationPolicy>();

            //enables [PostInject]
            Policies.Interceptors(new PostInjectInterceptorPolicy());

            Scan(scan => {
                scan.Assembly(GetType()
                    .Assembly);
                scan.WithDefaultConventions();
            });

            For<ILog>()
                .Add(ctx => LogManager.GetLogger(ctx.ParentType.Name))
                .AlwaysUnique();

            ForSingletonOf<ConfigManager>()
                .Use<ConfigManager>()
                .OnCreation(cnfg => InitConfigurationManger(cnfg));

            ForSingletonOf<SendReceiveCache>()
                .Use<SendReceiveCache>();

            ForSingletonOf<IHandlersProvider>()
                .Use<HandlersProvider>();

            ForSingletonOf<ErrorCache>()
                .Use<ErrorCache>();

        }

        /// <summary>
        /// Initializes the configuration manager.
        /// </summary>
        /// <param name="configManager">The configuration manager.</param>
        private void InitConfigurationManger(ConfigManager configManager) {

            Assembly assembly = GetType()
                .Assembly;
            var configJson = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("config.json", StringComparison.InvariantCultureIgnoreCase));


            if (!string.IsNullOrEmpty(configJson)) {
                using (var stream = assembly.GetManifestResourceStream(configJson)) {
                    if (stream != null) {
                        using (var reader = new StreamReader(stream)) {
                            string json = reader.ReadToEnd();
                            if (StringUtils.IsNotEmpty(json)) {
                                configManager.AddConfigJsonString(json);
                            }
                        }
                    }
                }
            }

            if (File.Exists("config.json")) {
                configManager.AddConfigFilePaths("config.json");
            }
        }
    }
}
