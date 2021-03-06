﻿namespace EzBobRest {
    using System.Threading;
    using Common.Logging;
    using EzBobCommon;
    using EzBobRest.Init;
    using Microsoft.Owin.Hosting;

    /// <summary>
    /// Self hosted http rest server
    /// </summary>
    public class RestServer {
        private readonly CountdownEvent latch = new CountdownEvent(1);
        private int isOpened = 0;
        private volatile bool isStarted = false;

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
                this.isStarted = true;
                this.latch.Wait();
                this.isStarted = false;
                Log.Info("rest server is stopped");
            }
        }

        public bool IsStarted
        {
            get { return this.isStarted; }
        }

        public void Stop() {
            if (Interlocked.CompareExchange(ref this.isOpened, 0, 1) == 0) {
                return;
            }

            this.latch.Signal();
        }
    }
}
