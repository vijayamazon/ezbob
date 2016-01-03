using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.DependencyResolution
{
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    class CommandHandlerConvention : IRegistrationConvention
    {
        public void Process(Type type, Registry registry) {
        
        }
    }
}
