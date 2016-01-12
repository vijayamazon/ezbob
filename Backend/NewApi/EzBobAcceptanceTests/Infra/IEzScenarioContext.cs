using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobAcceptanceTests.Infra
{
    using StructureMap;

    public interface IEzScenarioContext {
        void SetContainer(IContainer container);
        void SetRestIdStarted();
    }
}
