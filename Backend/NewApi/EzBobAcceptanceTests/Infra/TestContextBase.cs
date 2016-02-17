namespace EzBobAcceptanceTests.Infra {
    using System.Collections.Concurrent;
    using NServiceBus.AcceptanceTesting;
    using StructureMap;

    /// <summary>
    /// Implements basic functionality 
    /// </summary>
    public class TestContextBase : ScenarioContext, IEzScenarioContext {
        private readonly ConcurrentDictionary<string, IContainer> containers = new ConcurrentDictionary<string, IContainer>(); 
        private bool isRestedServerStarted = false;

        public bool IsRestServerStarted
        {
            get { return this.isRestedServerStarted; }
        }

        public IContainer GetContainer(string endPointName) {
            return this.containers[endPointName];
        }

        public void SetContainer(string endpointName, IContainer container) {
            this.containers[endpointName] = container;
        }

        public void SetRestServerStarted() {
            this.isRestedServerStarted = true;
        }
    }
}
