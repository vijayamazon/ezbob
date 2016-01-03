namespace EzBobRestTests
{
    using System;
    using StructureMap;

    public abstract class TestBase
    {
        /// <summary>
        /// Initializes the container, and scans assembly of provided type for registry
        /// </summary>
        /// <param name="scanAssemblyOfType">Type of the scan assembly of.</param>
        /// <returns></returns>
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
    }
}
