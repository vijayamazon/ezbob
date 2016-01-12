using System;
using System.Collections.Generic;

namespace EzBobAcceptanceTests.Infra {
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobRest;
    using EzBobRest.Init;
    using NServiceBus;
    using StructureMap;
    using StructureMap.Building.Interception;
    using StructureMap.Pipeline;
    using StructureMap.TypeRules;

    public class TestsInterceptionPolicy : IInterceptorPolicy {
        private IEzScenarioContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
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
             else if (pluginType.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))) {
                 int kk = 0;
             }
        }

        private RestServer WaitUntillRestServerStarted(RestServer restServer) {
            Task.Run(() => {
                while (!restServer.IsStarted) {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                this.Context.SetRestIdStarted();
            });

            return restServer;
        }
    }
}
