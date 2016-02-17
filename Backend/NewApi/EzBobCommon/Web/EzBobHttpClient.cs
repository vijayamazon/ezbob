namespace EzBobCommon.Web {
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Uses different HttpClient instances for each request
    /// </summary>
    public class EzBobHttpClient : IEzBobHttpClient {
        /// <summary>
        /// Downloads the page asynchronously as string.
        /// </summary>
        /// <param name="pageAddress">The page address.</param>
        /// <returns></returns>
        public async Task<string> DownloadPageAsyncAsString(string pageAddress) {
            HttpClient client = null;
            try {
                client = GetHttpClient();
                using (HttpResponseMessage response = await client.GetAsync(new Uri(pageAddress))) {
                    using (HttpContent content = response.Content) {
                        return await content.ReadAsStringAsync();
                    }
                }
            } finally {
                CallDispose(client);
            }
        }

        /// <summary>
        /// Downloads the page asynchronously as byte array.
        /// </summary>
        /// <param name="pageAddress">The page address.</param>
        /// <returns></returns>
        public async Task<byte[]> DownloadPageAsyncAsByteArray(string pageAddress) {
            HttpClient client = null;
            try {
                client = GetHttpClient();
                using (HttpResponseMessage response = await client.GetAsync(pageAddress)) {
                    using (HttpContent content = response.Content) {
                        return await content.ReadAsByteArrayAsync();
                    }
                }
            } finally {
                CallDispose(client);
            }
        }

        /// <summary>
        /// Posts the asynchronously and gets string response.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpContent">Content of the HTTP.</param>
        /// <returns></returns>
        public async Task<string> PostAsyncAndGetStringResponse(string url, HttpContent httpContent) {
            HttpClient client = null;
            try {
                client = GetHttpClient();
                using (HttpResponseMessage response = await client.PostAsync(url, httpContent)) {
                    using (HttpContent content = response.Content) {
                        return await content.ReadAsStringAsync();
                    }
                }
            } finally {
                CallDispose(client);
            }
        }

        /// <summary>
        /// Posts asynchronously and converts response to json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpContent">Content of the HTTP.</param>
        /// <returns></returns>
        public async Task<T> PostAsyncAndGetJsonReponseAs<T>(string url, HttpContent httpContent) where T : new() {
            HttpClient client = null;
            try {
                client = GetHttpClient();
                using (HttpResponseMessage response = await client.PostAsync(url, httpContent)) {
                    using (HttpContent content = response.Content) {
                        var stream = await content.ReadAsStreamAsync();
                        return CreateResponse<T>(stream);
                    }
                }
            } finally {
                CallDispose(client);
            }
        }

        /// <summary>
        /// Gets json response and converts it to specified T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public async Task<T> GetAsyncJsonResponseAs<T>(string url) where T : class, new() {
            HttpClient client = null;
            try {
                client = GetHttpClient();
                var stream = await client.GetStreamAsync(url);
                return CreateResponse<T>(stream);
            } finally {
                CallDispose(client);
            }
        }

        /// <summary>
        /// Creates the response.
        ///  Used when response is not array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private T CreateResponse<T>(Stream stream) where T : new() {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(stream)) {
                using (var jsonTextReader = new JsonTextReader(sr)) {
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <returns></returns>
        protected virtual HttpClient GetHttpClient() {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
//            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "utf-8");
            return httpClient;
        }

        /// <summary>
        /// Calls the dispose.
        /// </summary>
        /// <param name="client">The client.</param>
        protected virtual void CallDispose(HttpClient client) {
            if (client != null) {
                client.Dispose();
            }
        }
    }
}
