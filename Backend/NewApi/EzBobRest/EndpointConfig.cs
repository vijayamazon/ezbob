
namespace EzBobRest
{
    using System.Diagnostics;
    using NServiceBus;
    using NServiceBus.Logging;
    using NServiceBus.Persistence.Legacy;
    using StructureMap;

    /*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://particular.net/articles/the-nservicebus-host
	*/
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            InitLogging();
            InitConventions(configuration);

            Container container = new Container(c => c.AddRegistry<EzBobRestRegistry>());

            configuration.EndpointName("EzBobRestService");
            configuration.UseSerialization<JsonSerializer>();
            configuration.UseTransport<MsmqTransport>();
            configuration.EnableInstallers();

            if (Debugger.IsAttached)
            {
                configuration.UsePersistence<InMemoryPersistence>();
            }
            else
            {
                configuration.UsePersistence<MsmqPersistence>();
            }

            configuration.UseContainer<StructureMapBuilder>(c => c.ExistingContainer(container));
        }

        private void InitConventions(BusConfiguration config)
        {
            config.Conventions()
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("EzBobApi.Commands"));
        }

        private void InitLogging()
        {
            LogManager.Use<CommonLoggingFactory>();
//
//            LogManager.Use<Log4NetFactory>();
//
//            PatternLayout layout = new PatternLayout
//            {
//                ConversionPattern = "%d [%t] %-5p %c [%x] - %m%n"
//            };
//            layout.ActivateOptions();
//            ConsoleAppender consoleAppender = new ConsoleAppender
//            {
//                Threshold = Level.Debug,
//                Layout = layout
//            };
//            // Note that ActivateOptions is required in NSB 5 and above
//            consoleAppender.ActivateOptions();
//
//            BasicConfigurator.Configure(consoleAppender);

        }
    }
}
