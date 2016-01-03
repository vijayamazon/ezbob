using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests {
    using System.Threading;
    using EzBobApi.Commands;
    using EzBobApi.Commands.Customer;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobService;
    using EzBobService.Customer;
    using EzBobServiceTests.NSB;
    using global::NServiceBus.AcceptanceTesting;
    using Newtonsoft.Json;
    using NServiceBus;
    using NServiceBus.Logging;
    using NUnit.Framework;
    using StructureMap;
    using StructureMap.Graph;
    using StructureMap.Pipeline;

    [TestFixture]
    internal class NServiceBusTests : TestBase {
        [Test]
        public void NSBTest() {
            //initiates structure map container 
            IContainer container = InitContainer(typeof(CustomerProcessor));

            InfoAccumulator acc = new InfoAccumulator();
            acc.AddError("error1");
            acc.AddError("error2");
            acc.AddException(new InvalidOperationException("bla bla bla"));

            string json = JsonConvert.SerializeObject(acc);

            var info = JsonConvert.DeserializeObject(json, typeof(InfoAccumulator));


            Context ctx = new Context();

            Scenario.Define(ctx)
                .WithEndpoint<EzBobService>(c => c.CustomConfig(cfg => cfg
                    .UseContainer(CreateNsbTestContainer(container)) //based on initialized container, creates container suitable for NSB test framework
                    )
                    .Given((bus, context) => SubscriptionBehavior.OnEndpointSubscribed(s => {
                        int kk = 0;
                    }))
                    .When(context => true, bus => bus.Send("EzBobService", new CustomerSignupCommand())))
                .Done(c => true)
                .Run();
        }

        private class Context : ScenarioContext {
            private int countCompanyUpdate = 0;
            private bool isCustomerUpdated = false;

            public Context() {
                this.IsMaySignup = false;
            }

            public bool IsMaySignup { get; set; }
            public bool IsMayUpdateCustomer { get; private set; }

            public bool IsMayUpdateCompany
            {
                get { return this.isCustomerUpdated && this.countCompanyUpdate < 2; }
            }

            public bool IsFinished {
                get { return this.countCompanyUpdate >= 2; }
            }

            public void RegisterCompanyUpdate() {
                ++ this.countCompanyUpdate;
            }

            public void RegisterCustomerSignup() {
                this.IsMayUpdateCustomer = true;
            }

            public void RegisterCustomerUpdate() {
                this.isCustomerUpdated = true;
            }

        }

        [Test]
        public void TestScenario() {

            Context ctx = new Context();

            Scenario.Define(ctx)
                .WithEndpoint<EzBobService>(c => c
                    .Given((bus, context) =>
                        SubscriptionBehavior.OnEndpointSubscribed(s => {
                            if (s.SubscriberReturnAddress.Queue.Contains("EzBobService2")) {
                                context.IsMaySignup = true;
                            }
                        })))
                .WithEndpoint<RestService>(c => c
                    .Given((bus, context) => {
                        SubscriptionBehavior.OnEndpointSubscribed(s => {
                            if (s.SubscriberReturnAddress.Queue.Contains("EzBobService2")) {
                                context.IsMaySignup = true;
                            }
                        });

                        bus.Send("EzBobService2", new CustomerSignupCommand());
                    })
                    .When(context => context.IsMaySignup,
                        bus => {
                            bus.Send("EzBobService", new CustomerSignupCommand());
                        }))
                .Done(context => context.IsFinished)
                .Run();

            Thread.Sleep(1000 * 60 * 60);
        }

        public class TestPolicy1 : IFamilyPolicy
        {
            /// <summary>
            /// Allows you to create missing registrations for an unknown plugin type
            ///             at runtime.
            ///             Return null if this policy does not apply to the given type
            /// </summary>
            public PluginFamily Build(Type type)
            {
                if (type != typeof(log4net.ILog))
                {
                    return null;
                }

                var family = new PluginFamily(type);
                var instance = new ObjectInstance(log4net.LogManager.GetLogger(this.GetType()));

                family.SetDefault(instance);
                return family;
            }

            /// <summary>
            /// Should this policy be used to determine whether or not the Container has
            ///             registrations for a plugin type in the PluginGraph.HasFamily(type) method
            /// </summary>
            public bool AppliesToHasFamilyChecks { get; private set; }
        }

        private string CreateSighnupJson() {
            CustomerSignupCommand cmd = new CustomerSignupCommand();
            cmd.Account = new AccountInfo {
                EmailAddress = "aaa"
            };

            var res = JsonConvert.ToString(cmd);
            return res;
        }
    }
}
