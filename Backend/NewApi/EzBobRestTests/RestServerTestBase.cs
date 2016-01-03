namespace EzBobRestTests {
    using System;
    using System.Text;
    using System.Threading;
    using EzBobCommon.Configuration;
    using EzBobRest;
    using EzBobRestTests.Properties;
    using Microsoft.Owin.Security.OAuth;
    using StructureMap;

    public abstract class RestServerTestBase : TestBase {
        private Thread restServerThread;

        protected void StartTest(Action<IContainer> beforeStart, Action<RestServer> test) {
            var container = CreateContainer();
            var restServer = container.GetInstance<RestServer>();
            this.restServerThread = CreateServerThread(restServer);

            beforeStart(container);

            this.restServerThread.Start();

            Thread.Sleep(5000);

            try {
                test(restServer);
            } finally {
                if (restServer != null) {
                    restServer.Stop();
                }
            }
        }

        protected void StartTest(Action<RestServer> test) {
            var container = CreateContainer();
            var restServer = container.GetInstance<RestServer>();
            this.restServerThread = CreateServerThread(restServer);
            this.restServerThread.Start();

            Thread.Sleep(5000);
            try {
                test(restServer);
            } finally {
                if (restServer != null) {
                    restServer.Stop();
                }
            }
        }

        private Thread CreateServerThread(RestServer restServer) {
            var serverThread = new Thread(restServer.Start);
            serverThread.IsBackground = true;
            serverThread.Name = "TestServerThread";
            serverThread.SetApartmentState(ApartmentState.STA);
            return serverThread;
        }

        private IContainer CreateContainer() {
            IContainer container = InitContainer(typeof(RestServer));
            container.Configure(c => c.For<OAuthAuthorizationServerProvider>()
                .Use<TestOAuth2AuthorizationProvider>());

            var configManager = container.GetInstance<ConfigManager>();
            configManager.AddConfigJsonString(Encoding.UTF8.GetString(Resources.config));
            return container;
        }
    }
}
