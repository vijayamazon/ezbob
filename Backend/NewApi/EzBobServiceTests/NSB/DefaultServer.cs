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
    using NServiceBus.Timeout.Core;
    using StructureMap;
    using StructureMap.Configuration.DSL;

    /// <summary>
    /// Serves as a template for the NServiceBus configuration of an endpoint.
    /// You can do all sorts of fancy stuff here, such as support multiple transports, etc.
    /// Here it's stripped down to support just the defaults (MSMQ transport).
    /// </summary>
    public class DefaultServer<T> : IEndpointSetupTemplate where T : Registry, new()
    {
        public BusConfiguration GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource, Action<BusConfiguration> configurationBuilderCustomization)
        {
            var settings = runDescriptor.Settings;

//            var types = GetTypesToUse(endpointConfiguration);

            var config = new BusConfiguration();
//            config.EndpointName(endpointConfiguration.EndpointName);
            config.AssembliesToScan(typeof(T).Assembly, typeof(CustomerSignupCommand).Assembly);
            config.CustomConfigurationSource(configSource);
            config.UsePersistence<InMemoryPersistence>();
            config.PurgeOnStartup(true);
            config.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
            config.UseSerialization<JsonSerializer>();
            config.UseTransport<MsmqTransport>();
            config.DisableFeature<TimeoutManager>();//if not disabled there is null reference exception
//            config.EnableInstallers();

            // Plug-in a behavior that listens for subscription messages
            config.Pipeline.Register<SubscriptionBehavior.Registration>();
            config.RegisterComponents(c => c.ConfigureComponent<SubscriptionBehavior>(DependencyLifecycle.InstancePerCall));

            // Important: you need to make sure that the correct ScenarioContext class is available to your endpoints and tests
            config.RegisterComponents(r =>
            {
                r.RegisterSingleton(runDescriptor.ScenarioContext.GetType(), runDescriptor.ScenarioContext);
                r.RegisterSingleton(typeof(ScenarioContext), runDescriptor.ScenarioContext);
            });

            config.Conventions()
               .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("EzBobApi.Commands"));

            Container container = new Container(c => c.AddRegistry<T>());
            config.UseContainer<StructureMapBuilder>(c => c.ExistingContainer(container));

            // Call extra custom action if provided
            if (configurationBuilderCustomization != null)
            {
                configurationBuilderCustomization(config);
            }

           

            return config;
        }

        static IEnumerable<Type> GetTypesToUse(EndpointConfiguration endpointConfiguration)
        {
            var assemblies = new AssemblyScanner().GetScannableAssemblies();

            var types = assemblies.Assemblies
                //exclude all test types by default
                                  .Where(a => a != Assembly.GetExecutingAssembly())
                                  .SelectMany(a => a.GetTypes());


            types = types.Union(GetNestedTypeRecursive(endpointConfiguration.BuilderType.DeclaringType, endpointConfiguration.BuilderType));

            types = types.Union(endpointConfiguration.TypesToInclude);

            return types.Where(t => !endpointConfiguration.TypesToExclude.Contains(t)).ToList();
        }

        static IEnumerable<Type> GetNestedTypeRecursive(Type rootType, Type builderType)
        {
            if (rootType == null) {
                yield break;
            }

            yield return rootType;

            if (typeof(IEndpointConfigurationFactory).IsAssignableFrom(rootType) && rootType != builderType)
                yield break;

            foreach (var nestedType in rootType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SelectMany(t => GetNestedTypeRecursive(t, builderType)))
            {
                yield return nestedType;
            }
        }
    }
}
