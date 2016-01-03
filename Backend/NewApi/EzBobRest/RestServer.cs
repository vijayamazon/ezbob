namespace EzBobRest {
    using System.Threading;
    using EzBobCommon;
    using EzBobRest.Init;
    using log4net;
    using Microsoft.Owin.Hosting;

    /// <summary>
    /// Self hosted http rest server
    /// </summary>
    public class RestServer {
        private readonly CountdownEvent latch = new CountdownEvent(1);
        private int isOpened = 0;

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public IStartup Startup { get; set; }

        public void Start() {
            if (Interlocked.CompareExchange(ref this.isOpened, 1, 0) == 1) {
                return;
            }

            using (WebApp.Start(Config.ServerAddress, Startup.Configuration)) {
                Log.Info("started rest server");
                this.latch.Wait();
                Log.Info("rest server is stopped");
            }
        }

        public void Stop() {
            if (Interlocked.CompareExchange(ref this.isOpened, 0, 1) == 0) {
                return;
            }

            this.latch.Signal();
        }
    }
}
