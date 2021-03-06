﻿namespace EzBobAcceptanceTests.Infra {
    using EzBob3dPartiesApi.Amazon;
    using EzBobApi.Commands.Customer;
    using global::EzBobService;
    using NServiceBus.AcceptanceTesting;

    /// <summary>
    /// using <see cref="DefaultServer{T}"/> to setup EzBobService endpoint
    /// </summary>
    public class EzBobService : EndpointConfigurationBuilder {

        public static string EndpointName { get; private set; }

        static EzBobService() {
            EndpointName = "EzBobService2";
        }

        public EzBobService() {
            EndpointSetup<DefaultServer<EzBobServiceRegistry>>()
                .CustomEndpointName(EndpointName)
                .IncludeType<AmazonGetOrders3dPartyCommand>()
                .IncludeType<CustomerSignupCommand>();
        }
    }
}
