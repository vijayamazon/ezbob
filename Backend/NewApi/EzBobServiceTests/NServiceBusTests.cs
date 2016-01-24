using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests {
    using System.Diagnostics;
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

            container = new Container(new EzBobServiceRegistry());


            Context context = new Context();

            Scenario.Define(context)
                .WithEndpoint<EzBobService>(b => {
                    b.Given((bus, ctx) => {
                        bus.Send<CustomerSignupCommand>("EzBobService2", m => {
                            m.MessageId = Guid.NewGuid();
                        });
                        SubscriptionForIncomingBehavior.OnEndpointSubscribed(s => {
                            int kk = 0;
                        });

                        OutgoingSubscription.OnEndpointSubscribed(s => {
                            int kk = 0;
                        });
                    })
                        .When(ctx => ctx.IsServiceReady, bus => bus.Send<CustomerSignupCommand>("EzBobService2", m => {
                            m.MessageId = Guid.NewGuid();
                        }));
                })
                .Done(IsDone)
                .Run();

//
//            Scenario.Define(ctx)
//                .WithEndpoint<EzBobService>(c => c.CustomConfig(cfg => cfg
//                    .UseContainer(CreateNsbTestContainer(container)) //based on initialized container, creates container suitable for NSB test framework
//                    )
//                    .Given((bus, context) => SubscriptionForIncomingBehavior.OnEndpointSubscribed(s => {
//                        int kk = 0;
//                    }))
//                    .When(context => context.IsMaySignup, bus => bus.Send("EzBobService2", new CustomerSignupCommand())))
//                .Done(IsDone)
//                .Run();
        }

        private bool IsDone(Context ctx) {
            return ctx.IsFinished;
        }

        private class Context : ScenarioContext {
            private int countCompanyUpdate = 0;
            private bool isCustomerUpdated = false;

            private bool isMaySignup = false;


            public Context() {
                this.IsMaySignup = false;
            }

            public bool IsServiceReady { get; set; }

            public bool IsMaySignup
            {
                get
                {
                    bool b = this.isMaySignup;
                    return b;
                }
                set { this.isMaySignup = value; }
            }

            public bool IsMayUpdateCustomer { get; private set; }

            public bool IsMayUpdateCompany
            {
                get { return this.isCustomerUpdated && this.countCompanyUpdate < 2; }
            }

            public bool IsFinished
            {
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
                    .Given((bus, context) => context.IsMaySignup = true))
                .WithEndpoint<RestService>(c => c
                    .When(context => context.IsMaySignup,
                        bus => {
                            bus.Send("EzBobService2", new CustomerSignupCommand());
                        }))
                .Done(context => context.IsFinished)
                .Run();

            Thread.Sleep(1000 * 60 * 60);
        }
    }
}
