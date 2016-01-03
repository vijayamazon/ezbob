using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests.NSB {
    using EzBobApi.Commands;
    using EzBobApi.Commands.Customer.Sections;
    using global::EzBobService;
    using global::NServiceBus.AcceptanceTesting;

    public class EzBobService : EndpointConfigurationBuilder {
        public EzBobService() {
            EndpointSetup<DefaultServer<EzBobServiceRegistry>>()
                .CustomEndpointName("EzBobService2");

//                .AddMapping<CustomerInfo>(typeof(EzBobService));
        }
    }
}
