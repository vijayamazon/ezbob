namespace EzBobAcceptanceTests.Infra {
    using System;
    using System.Linq;
    using EzBobCommon.NSB;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.Config.ConfigurationSource;
    using NServiceBus.Features;
    using NServiceBus.Logging;
    using StructureMap;
    using StructureMap.Configuration.DSL;

    /// <summary>
    /// Serves as a template for the NServiceBus configuration of an endpoint.
    /// </summary>
    public class DefaultServer<T> : IEndpointSetupTemplate
        where T : Registry, new() {
        public BusConfiguration GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource, Action<BusConfiguration> configurationBuilderCustomization) {
            var settings = runDescriptor.Settings;

            LogManager.Use<CommonLoggingFactory>();

            var config = new BusConfiguration();

            Container container = new Container(c => {
                c.AddRegistry<T>();
                c.Scan(scan => scan.WithDefaultConventions());
            });

            config.UseContainer<StructureMapBuilder>(c => c.ExistingContainer(container));
            IEzScenarioContext ctx;
            ctx = runDescriptor.ScenarioContext as IEzScenarioContext;
            if (ctx != null) {
                ctx.SetContainer(endpointConfiguration.EndpointName, container);
                container.Model.Pipeline.Policies.Interceptors.Add(new TestsInterceptionPolicy(ctx));
            }

            config.EndpointName(endpointConfiguration.EndpointName);
            config.AssembliesToScan(Enumerable.Repeat(typeof(T).Assembly, 1)
                .Concat(Enumerable.Repeat(this.GetType()
                    .Assembly, 1))
                .Concat(endpointConfiguration.TypesToInclude.Select(o => o.Assembly))
                .Distinct());
            config.CustomConfigurationSource(configSource);
            config.UsePersistence<InMemoryPersistence>();
            config.EnableFeature<MsmqSubscriptionPersistence>();
            config.PurgeOnStartup(true);
            config.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
            config.UseSerialization<JsonSerializer>();
            config.UseTransport<MsmqTransport>();
            config.DisableFeature<TimeoutManager>(); //if not disabled there is null reference exception
            config.EnableInstallers();
            config.EnableFeature<AutoSubscribe>();

            config.RegisterComponents(c => c.ConfigureComponent<AsyncHandlerSupport>(DependencyLifecycle.SingleInstance));

//            // Plug-in a behavior that listens for subscription messages
//            config.Pipeline.Register<SubscriptionForIncomingBehavior.Registration>();
//            config.Pipeline.Register<OutgoingSubscription.Registration>();
//            config.RegisterComponents(c => c.ConfigureComponent<OutgoingSubscription>(DependencyLifecycle.InstancePerCall));
//            config.RegisterComponents(c => c.ConfigureComponent<SubscriptionForIncomingBehavior>(DependencyLifecycle.InstancePerCall));

            // Important: you need to make sure that the correct ScenarioContext class is available to your endpoints and tests
            config.RegisterComponents(r => {
                r.RegisterSingleton(runDescriptor.ScenarioContext.GetType(), runDescriptor.ScenarioContext);
                r.RegisterSingleton(typeof(ScenarioContext), runDescriptor.ScenarioContext);
            });

            config.Conventions()
                .DefiningCommandsAs(t => (t.Namespace != null && t.Namespace.StartsWith("EzBobApi.Commands")) ||
                    typeof(CommandBase).IsAssignableFrom(t));

            // Call extra custom action if provided
            if (configurationBuilderCustomization != null) {
                configurationBuilderCustomization(config);
            }

            return config;
        }
    }
}
