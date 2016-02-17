using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobAcceptanceTests.Infra
{
    using StructureMap;

    public interface IEzScenarioContext {
        void SetContainer(string endpointName, IContainer container);
        void SetRestServerStarted();
    }
}
