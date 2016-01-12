namespace EzBobAcceptanceTests.Infra {
    using EzBobApi.Commands.Customer;
    using EzBobRest;
    using NServiceBus.AcceptanceTesting;

    internal class RestService : EndpointConfigurationBuilder {
        public RestService() {
            EndpointSetup<DefaultServer<EzBobRestRegistry>>()
                .CustomEndpointName("EzBobRestService")
                .IncludeType<CustomerSignupCommand>();
        }
    }
}
