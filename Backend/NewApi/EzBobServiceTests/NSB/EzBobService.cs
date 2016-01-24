namespace EzBobServiceTests.NSB {
    using EzBobApi.Commands.Customer;
    using global::EzBobService;
    using NServiceBus.AcceptanceTesting;

    /// <summary>
    /// using <see cref="DefaultServer"/> to setup EzBobService endpoint
    /// </summary>
    public class EzBobService : EndpointConfigurationBuilder {
        public EzBobService() {
            EndpointSetup<DefaultServer<EzBobServiceRegistry>>()
                .CustomEndpointName("EzBobService2")
                .IncludeType<CustomerSignupCommand>();
        }
    }
}
