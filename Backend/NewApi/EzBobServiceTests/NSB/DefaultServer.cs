using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests.NSB
{
    using System.Reflection;
    using EzBobApi.Commands.Customer;
    using global::NServiceBus;
    using global::NServiceBus.AcceptanceTesting;
    using global::NServiceBus.AcceptanceTesting.Support;
    using global::NServiceBus.Config.ConfigurationSource;
    using global::NServiceBus.Hosting.Helpers;
    using NServiceBus.Features;
    using NServiceBus.Logging;
    using NServiceBus.Timeout.Core;
    using StructureMap;
    using StructureMap.Configuration.DSL;

    /// <summary>
    /// Serves as a template for the NServiceBus configuration of an endpoint.
    /// </summary>
    public class DefaultServer<T> : IEndpointSetupTemplate where T : Registry, new()
    {
        public BusConfiguration GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource, Action<BusConfiguration> configurationBuilderCustomization) {
            var settings = runDescriptor.Settings;

            LogManager.Use<CommonLoggingFactory>();

            var config = new BusConfiguration();

            Container container = new Container(c => c.AddRegistry<T>());
            config.UseContainer<StructureMapBuilder>(c => c.ExistingContainer(container));

            config.EndpointName(endpointConfiguration.EndpointName);
            config.AssembliesToScan(Enumerable.Repeat(typeof(T).Assembly, 1)
                .Concat(Enumerable.Repeat(this.GetType().Assembly, 1))
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

            // Plug-in a behavior that listens for subscription messages
            config.Pipeline.Register<SubscriptionForIncomingBehavior.Registration>();
            config.Pipeline.Register<OutgoingSubscription.Registration>();
            config.RegisterComponents(c => c.ConfigureComponent<OutgoingSubscription>(DependencyLifecycle.InstancePerCall));
            config.RegisterComponents(c => c.ConfigureComponent<SubscriptionForIncomingBehavior>(DependencyLifecycle.InstancePerCall));

            // Important: you need to make sure that the correct ScenarioContext class is available to your endpoints and tests
            config.RegisterComponents(r => {
                r.RegisterSingleton(runDescriptor.ScenarioContext.GetType(), runDescriptor.ScenarioContext);
                r.RegisterSingleton(typeof(ScenarioContext), runDescriptor.ScenarioContext);
            });

            config.Conventions()
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("EzBobApi.Commands"));

            // Call extra custom action if provided
            if (configurationBuilderCustomization != null) {
                configurationBuilderCustomization(config);
            }

            return config;
        }
    }
}
