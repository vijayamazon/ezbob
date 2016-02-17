using NServiceBus;

namespace EzBobCommon.NSB
{
    using NServiceBus.Logging;
    using StructureMap;

    public abstract class EndpointConfigBase : IConfigureThisEndpoint
    {
        /// <summary>
        /// Allows to override default settings.
        /// </summary>
        /// <param name="configuration">Endpoint configuration builder.</param>
        public virtual void Customize(BusConfiguration configuration) {
            InitLogging();
            InitNSBConventions(configuration);
            configuration.UseContainer<StructureMapBuilder>(c => c.ExistingContainer(GetContainer()));
            configuration.RegisterComponents(components => components.ConfigureComponent<AsyncHandlerSupport>(DependencyLifecycle.SingleInstance));
        }

        protected abstract IContainer GetContainer(); 

        private void InitLogging()
        {
            LogManager.Use<CommonLoggingFactory>();
        }

        private void InitNSBConventions(BusConfiguration config)
        {
            config.Conventions()
                .DefiningCommandsAs(t => (t.Namespace != null && t.Namespace.StartsWith("EzBobApi.Commands"))
                    || typeof(CommandBase).IsAssignableFrom(t));
        }


    }
}
