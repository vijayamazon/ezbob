using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests.NSB {
    using System.Reflection;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.ObjectBuilder.Common;
    using NServiceBus.Settings;

    public static class NSBTestExtensions {
        public static IScenarioWithEndpointBehavior<TContext> WithEndpoint<T, TContext>(this IScenarioWithEndpointBehavior<TContext> scenario, Action<EndpointBehaviorBuilder<TContext>> behavior) where T : EndpointConfigurationBuilder where TContext : ScenarioContext {
            return null;
        }
       
    }
}
