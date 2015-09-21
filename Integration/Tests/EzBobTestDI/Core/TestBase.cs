namespace EzBobTestDI.Core {
    using System;
    using StructureMap;

    public class TestBase {
          protected IContainer InitContainer(Type scanAssemblyOfType) {
            Container container = new Container();
            container.Configure(c => c.Scan(scanner => {
                scanner.AssemblyContainingType(scanAssemblyOfType);
                scanner.LookForRegistries();
            }));

            return container;
        }
    }
}
