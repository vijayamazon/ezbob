namespace EzBobAcceptanceTests.Infra {
    using EzBobCommon;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.MessageMutator;

    public class ServiceInspector : IMutateOutgoingMessages, IMutateIncomingMessages, INeedInitialization {
        /// <summary>
        /// Mutates the given message just before it's serialized
        /// </summary>        
        public object MutateOutgoing(object message) {
            return message;
        }

        /// <summary>
        /// Mutates the given message right after it has been deserialized
        /// </summary>
        public object MutateIncoming(object message) {
            return message;
        }

        [Injected]
        public ScenarioContext Context { get; set; }

        /// <summary>
        /// Allows to override default settings.
        /// </summary>
        /// <param name="configuration">Endpoint configuration builder.</param>
        public void Customize(BusConfiguration configuration) {
            configuration.RegisterComponents(c => c.ConfigureComponent<ServiceInspector>(DependencyLifecycle.InstancePerCall));
        }
    }
}