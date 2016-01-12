namespace EzBobAcceptanceTests.Customer {
    using System;
    using System.Net.Http;
    using System.Text;
    using EzBobAcceptanceTests.Customer.Requests;
    using EzBobAcceptanceTests.Customer.Responses;
    using EzBobAcceptanceTests.Infra;
    using EzBobAcceptanceTests.Infra.Utils;
    using EzBobApi.Commands.Customer;
    using EzBobCommon.Http;
    using EzBobService.Customer;
    using Newtonsoft.Json;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NUnit.Framework;
    using StructureMap;

    [TestFixture]
    public class CustomerTests {
        [Test]
        public void SignupCustomerTest() {
            Context ctx = new Context();

            IContainer serviceContainer = null;
            IContainer restContainer = null;

            RestClient restClient = new RestClient();

            HttpClient httpClient = null;

            Scenario.Define(ctx)
                .WithEndpoint<EzBobService>(c => c
                    .Given((bus, context) => {
                        serviceContainer = context.Container;
                        context.IsMaySignup = true;
                    }))
                .WithEndpoint<RestService>(c => c
                    .Given((bus, context) => {
                        restContainer = context.Container;
                        DecorateHandlers(serviceContainer);
                    })
                    .When(context => context.IsRestServerStarted && context.IsMaySignup,
                        (bus, contxt) => {
                            TestSameEmailDoubleSignup(bus, contxt);
                        }))
                .Done(context => context.IsDone)
                .Run();
        }

        /// <summary>
        /// Tests the same email double sign-up.
        /// </summary>
        /// <param name="bus">The bus.</param>
        /// <param name="ctx">The context.</param>
        private void TestSameEmailDoubleSignup(IBus bus, Context ctx) {

            string emailAddress = Generator.GetRandomEmailAddress();

            RestClient restClient = new RestClient();
            var httpClient = restClient.GetClient();
            var response = SendSignupRequest(httpClient, emailAddress);
            Assert.IsNotEmpty(response, "got empty response");

            var signupResponse = JsonConvert.DeserializeObject<CustomerSignupResponse>(response);
            Assert.IsTrue(signupResponse.SelfValidate(), "invalid response format");
            Assert.IsFalse(signupResponse.HasErrors, "expected no errors");

            response = SendSignupRequest(httpClient, emailAddress);
            Assert.IsNotEmpty(response, "got empty response");

            signupResponse = JsonConvert.DeserializeObject<CustomerSignupResponse>(response);
            Assert.IsTrue(signupResponse.SelfValidate(), "invalid response format");
            Assert.IsTrue(signupResponse.HasErrors, "expected to get errors");

            ctx.IsDone = true;//finish test
        }

        private string SendSignupRequest(HttpClient client, string emailAddress) {
            SignupRequest request = new SignupRequest(emailAddress);
            string json = JsonConvert.SerializeObject(request);
            HttpResponseMessage responseMessage = client.PostAsync("http://localhost:12345/api/v1/customer/signup", new JsonContent(json))
                .Result;
            string response = responseMessage.Content.ReadAsStringAsync()
                .Result;
            return response;
        }

        private void ValidateSignupResponse(string response, Context ctx) {
            int kk = 0;
            ctx.IsDone = true;
        }

        private void DecorateHandlers(IContainer container) {
            try { 
                var sch = container.GetInstance<SignupCustomerHandler>();
                var handler = container.GetInstance<IHandleMessages<CustomerSignupCommand>>();
                var handlers = container.GetAllInstances(typeof(IHandleMessages<CustomerSignupCommand>));
                int kk = 0;
            } catch (Exception ex) {
                int kk = 0;
            }
        }
    }

    internal class Context : ScenarioContext, IEzScenarioContext {
        public bool IsMaySignup { get; set; }
        public bool IsDone { get; set; }
        public bool IsRestServerStarted { get; private set; }

        private IContainer container;

        public IContainer Container {
            get { return this.container; }
        }

        public bool IsSignupSent { get; set; }

        public void SetContainer(IContainer container) {
            this.container = container;
        }

        public void SetRestIdStarted() {
            this.IsRestServerStarted = true;
        }
    }
}
