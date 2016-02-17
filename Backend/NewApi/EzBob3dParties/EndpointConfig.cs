
namespace EzBob3dParties {
    using EzBobCommon.NSB;
    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Persistence.Legacy;
    using StructureMap;

    /*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://particular.net/articles/the-nservicebus-host
	*/

    public class EndpointConfig : EndpointConfigBase {
        public override void Customize(BusConfiguration configuration) {
            base.Customize(configuration);

            configuration.EndpointName("EzBob3dParties");
            configuration.UseSerialization<JsonSerializer>();
            configuration.UseTransport<MsmqTransport>();
            configuration.EnableInstallers();

            configuration.EnableFeature<InMemoryTimeoutPersistence>();//msmq persistence does not provide timeout persistence
            configuration.UsePersistence<MsmqPersistence>();
        }

        protected override IContainer GetContainer() {
            Container container = new Container(c => c.AddRegistry<EzBob3dPartiesRegistry>());
            return container;
        }
    }
}
