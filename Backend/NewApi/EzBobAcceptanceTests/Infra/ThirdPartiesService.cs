namespace EzBobAcceptanceTests.Infra {
    using EzBob3dParties;
    using EzBob3dPartiesApi.Amazon;
    using EzBobApi.Commands.Customer;
    using NServiceBus.AcceptanceTesting;

    internal class ThirdPartiesService : EndpointConfigurationBuilder {

        public static string EndpointName { get; private set; }

        static ThirdPartiesService() {
            EndpointName = "EzBob3dParties";
        }

        public ThirdPartiesService() {
            EndpointSetup<DefaultServer<EzBob3dPartiesRegistry>>()
                .CustomEndpointName(EndpointName)
                .IncludeType<AmazonGetOrders3dPartyCommand>()
                .IncludeType<CustomerSignupCommand>();
        }
    }
}
