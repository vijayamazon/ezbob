namespace EzBobRest
{
    using System.Threading;
    using EzBobCommon;
    using NServiceBus;

    /// <summary>
    /// Starts the Rest server after NSB initialized
    /// </summary>
    public class EzBobRestServerStarter : IWantToRunWhenBusStartsAndStops
    {
        private static Thread restServerThread;
        private static readonly object syncLock = new object();

        [Injected]
        public RestServer RestServer { get; set; }
        /// <summary>
        /// Method called at startup.
        /// </summary>
        public void Start()
        {
            lock (syncLock)
            {
                if (restServerThread == null)
                {
                    restServerThread = CreateServerThread(RestServer);
                    restServerThread.Start();
                }
            }
        }

        /// <summary>
        /// Method called on shutdown.
        /// </summary>
        public void Stop()
        {
            lock (syncLock)
            {
                if (restServerThread != null)
                {
                    RestServer.Stop();
                }
            }
        }

        /// <summary>
        /// Creates the server thread.
        /// </summary>
        /// <param name="restServer">The rest server.</param>
        /// <returns></returns>
        private Thread CreateServerThread(RestServer restServer)
        {
            var serverThread = new Thread(restServer.Start);
            serverThread.IsBackground = true;
            serverThread.SetApartmentState(ApartmentState.STA);
            return serverThread;
        }
    }
}
