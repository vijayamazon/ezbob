using System;
using System.Collections.Generic;

namespace EzBobAcceptanceTests.Infra {
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobRest;
    using StructureMap.Building.Interception;
    using StructureMap.Pipeline;

    public class TestsInterceptionPolicy : IInterceptorPolicy {
        private IEzScenarioContext context;

        /// <summary>
        /// This is a place to intercept object created by 'structure map' before its use.
        /// Currently used to notify when REST server is up and ready.  
        /// </summary>
        public TestsInterceptionPolicy(IEzScenarioContext scenarioContext) {
            this.context = scenarioContext;
        }

        public string Description
        {
            get { return "Handlers Interceptor"; }
        }

        public IEzScenarioContext Context
        {
            get { return this.context; }
        }

        public IEnumerable<IInterceptor> DetermineInterceptors(Type pluginType, Instance instance) {
            if (pluginType == typeof(RestServer)) {
                yield return new FuncInterceptor<RestServer>((ctx, server) => WaitUntillRestServerStarted(server));
            }
        }

        private RestServer WaitUntillRestServerStarted(RestServer restServer) {
            Task.Run(() => {
                while (!restServer.IsStarted) {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                this.Context.SetRestServerStarted();
            });

            return restServer;
        }
    }
}
