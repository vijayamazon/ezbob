using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests.NSB {
    using EzBobRest;
    using NServiceBus.AcceptanceTesting;

    internal class RestService : EndpointConfigurationBuilder {
        public RestService() {
            EndpointSetup<DefaultServer<EzBobRestRegistry>>().CustomEndpointName("EzBobRestService");
        }
    }
}
