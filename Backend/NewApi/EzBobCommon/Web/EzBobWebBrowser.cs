namespace EzBobCommon.Web {
    using System;
    using System.Net.Http;

    /// <summary>
    /// Uses the same HttpClient instance for all requests
    /// </summary>
    public class EzBobWebBrowser : EzBobHttpClient, IEzBobWebBrowser {
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Sets the base address.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void SetBaseAddress(string url) {
            this.client.BaseAddress = new Uri(url);
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <returns></returns>
        protected override HttpClient GetHttpClient() {
            return this.client;
        }

        protected override void CallDispose(HttpClient client) {
            //DO NOTHING
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            this.client.Dispose();
        }
    }
}
