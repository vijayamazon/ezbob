namespace EchoSignLib.Rest {
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using EchoSignLib.EchoSignService;
    using EchoSignLib.Rest.Api;
    using Newtonsoft.Json;

    /// <summary>
    /// Echo-sign REST client.(Check out readme.txt).
    /// </summary>
    internal class EchoSignRestClient {

        private static readonly Uri baseRestUrl = new Uri("https://api.na1.echosign.com/api/rest/v5/");

        private static readonly Uri tokenEndpoint = new Uri("https://secure.echosign.com/oauth/token");
        private static readonly Uri tokenRefreshEndpoint = new Uri("https://secure.echosign.com/oauth/refresh");
        private static readonly Uri agreementsEndpoint = new Uri("https://secure.na1.echosign.com/api/rest/v5/agreements");
        private static readonly Uri transientDocumentsEndpoint = new Uri("https://secure.na1.echosign.com/api/rest/v5/transientDocuments");

        private readonly string refreshToken;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string redirectUri;

        private EchoSignRefreshAccessTokenResponse lastRefreshAccessTokenResponse;


        /// <summary>
        /// Initializes a new instance of the <see cref="EchoSignRestClient" /> class.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        public EchoSignRestClient(string refreshToken, string clientId, string clientSecret, string redirectUri) {
            this.refreshToken = refreshToken;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUri = redirectUri;
        }

        /// <summary>
        /// Sends the agreement.
        /// </summary>
        /// <param name="dci">The dci.</param>
        /// <returns></returns>
        public async Task<EchoSignSendAgreementResponse> SendAgreement(DocumentCreationInfo dci) {
            var fileInfo = dci.fileInfos[0];
            var docId = await UploadTransientDocumentAndGetId(fileInfo.fileName, fileInfo.file);
            var httpContent = new EchoSignSendAgreementRequest(dci)
                .WithTransientDocId(docId)
                .BuildContent();

            var accessToken = await GetAccessToken();
            var response = await Post<EchoSignSendAgreementResponse>(agreementsEndpoint, httpContent, accessToken);
            return response;
        }

        /// <summary>
        /// Uploads the transient document and get identifier.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        public async Task<string> UploadTransientDocumentAndGetId(string fileName, byte[] fileContent) {
            string accessToken = await GetAccessToken();

            var uploadContent = new EchoSignUploadTransientDocumentRequest()
                .WithFile(fileName, fileContent)
                .BuildContent();

            var response = await Post<EchoSignUploadTransientDocumentResponse>(transientDocumentsEndpoint, uploadContent, accessToken);
            return response.transientDocumentId;
        }

        /// <summary>
        /// Gets the agreement status.
        /// </summary>
        /// <param name="agreementId">The agreement identifier.</param>
        /// <returns></returns>
        public async Task<EchoSignAgreementStatusResponse> GetAgreementStatus(string agreementId) {
            string accessToken = await GetAccessToken();

            string path = "agreements/" + agreementId;

            var response = await Get<EchoSignAgreementStatusResponse>(accessToken, baseRestUrl, path);
            return response;
        }

        /// <summary>
        /// Gets the agreement document.
        /// </summary>
        /// <param name="agreementId">The agreement identifier.</param>
        /// <returns></returns>
        public async Task<EchoSignAgreementDocumentResponse> GetAgreementDocument(string agreementId)
        {
            string path = string.Format("agreements/{0}/combinedDocument?attachSupportingDocuments=true", agreementId);

            string accessToken = await GetAccessToken();

            var httpClient = GetHttpClient(accessToken);
            httpClient.BaseAddress = baseRestUrl;

            byte[] file;

            using (var httpResponse = await httpClient.GetAsync(path)) {
                using (var content = httpResponse.Content) {
                    file = await content.ReadAsByteArrayAsync();
                }
            }

            EchoSignAgreementDocumentResponse response = new EchoSignAgreementDocumentResponse {
                Content = file,
                MimeType = "application/pdf"
            };

            return response;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken() {

            if (this.lastRefreshAccessTokenResponse == null ||
                this.lastRefreshAccessTokenResponse.IsTimeOut()) {
                var httpContent = new EchoSignRefreshAccessTokenRequest()
                    .WithClientId(this.clientId)
                    .WithClientSecret(this.clientSecret)
                    .WithRefreshToken(this.refreshToken)
                    .BuildContent();

                this.lastRefreshAccessTokenResponse = await Post<EchoSignRefreshAccessTokenResponse>(tokenRefreshEndpoint, httpContent);
            }
            return this.lastRefreshAccessTokenResponse.access_token;
        }

        /// <summary>
        /// Gets the refresh token.(look at readme.txt how to obtain it).
        /// </summary>
        /// <param name="code">The authorization code. (look at readme.txt how to obtain it)</param>
        /// <returns></returns>
        public async Task<EchoSignRefreshTokenResponse> GetRefreshToken(string code) {
            var httpContent = new EchoSignRefreshTokenRequest()
                .WithCode(code)
                .WithClientId(this.clientId)
                .WithClientSecret(this.clientSecret)
                .WithRedirectUri(this.redirectUri)
                .BuildContent();

            return await Post<EchoSignRefreshTokenResponse>(tokenEndpoint, httpContent);
        }

        /// <summary>
        /// Posts to the specified URL and returns parsed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="uri">The URI.</param>
        /// <param name="httpContent">Content of the HTTP.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public async Task<TResponse> Post<TResponse>(Uri uri, HttpContent httpContent, string accessToken = null) where TResponse : class, new() {
            var client = GetHttpClient(accessToken);

            using (var response = await client.PostAsync(uri, httpContent)) {
                using (HttpContent content = response.Content) {
                    var stream = await content.ReadAsStreamAsync();
                    return CreateResponse<TResponse>(stream);
                }
            }
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="accessToken">The access token.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public async Task<TResponse> Get<TResponse>(string accessToken, Uri baseUrl, string path) where TResponse : class, new() {
            var client = GetHttpClient(accessToken);
            client.BaseAddress = baseUrl;
            using (var response = await client.GetAsync(path)) {
                using (HttpContent content = response.Content) {
                    var stream = await content.ReadAsStreamAsync();
                    return CreateResponse<TResponse>(stream);
                }
            }
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        private HttpClient GetHttpClient(string accessToken = null) {
            HttpClient client = new HttpClient();
            if (accessToken != null) {
                client.DefaultRequestHeaders.Add("Access-Token", accessToken);
            }

            return client;
        }

        /// <summary>
        /// Creates the response.
        ///  Used when response is not array
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private TResponse CreateResponse<TResponse>(Stream stream) where TResponse : class, new() {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(stream)) {
                using (var jsonTextReader = new JsonTextReader(sr)) {
                    return serializer.Deserialize<TResponse>(jsonTextReader);
                }
            }
        }
    }
}
