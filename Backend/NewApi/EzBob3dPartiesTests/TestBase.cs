using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesTests
{
    using StructureMap;

    public class TestBase
    {
        protected IContainer InitContainer(Type scanAssemblyOfType)
        {
            Container container = new Container();
            container.Configure(c => c.Scan(scanner =>
            {
                scanner.AssemblyContainingType(scanAssemblyOfType);
                scanner.LookForRegistries();
            }));

            return container;
        }

        protected Task<T> CreateCompletedTask<T>(T result) {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResult(result);
            return taskSource.Task;
        }
    }
}
