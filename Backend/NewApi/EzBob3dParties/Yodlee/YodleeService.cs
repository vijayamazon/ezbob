namespace EzBob3dParties.Yodlee {
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBob3dParties.Yodlee.Models;
    using EzBob3dParties.Yodlee.Models.SiteAccount;
    using EzBob3dParties.Yodlee.RequestResponse;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobCommon.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// The Yodlee service
    /// </summary>
    public class YodleeService {

        [Injected]
        public YodleeConfig Config { get; set; }

        [Injected]
        public IEzBobWebBrowser Browser { get; set; }

        [Injected]
        public ILog Log { get; set; }


        //called ones after instance is create
        //TODO: should be convert to IInstancePolicy when we will upgrade to SructureMap 4
        internal void InitAfterInject() {
            Browser.SetBaseAddress(Config.RestUrl);
        }

        /// <summary>
        /// Logins the co-brand.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal Task<YCobrandLoginResponse> LoginCobrand(YCobrandLoginRequest request) {
            HttpContent content = CreateHttpContent(request);

            return Browser.PostAsyncAndGetJsonReponseAs<YCobrandLoginResponse>(YodleeCommands.COB_LOGIN, content);
        }

        /// <summary>
        /// Logins the user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal Task<YUserLoginResponse> LoginUser(YUserLoginRequest request) {
            HttpContent content = CreateHttpContent(request);

            return Browser.PostAsyncAndGetJsonReponseAs<YUserLoginResponse>(YodleeCommands.USER_LOGIN, content);

        }

        internal Task<YTransactionsSearchResponse> GetTransactions2(YTransactionsSearchRequest request) {
            HttpContent content = CreateHttpContent(request);
            return Browser.PostAsyncAndGetJsonReponseAs<YTransactionsSearchResponse>(YodleeCommands.EXECUTE_USER_SEARCH_REQUEST, content);
        }

        internal YTransactionsSearchResponse GetTransactions3(YTransactionsSearchRequest request)
        {
            HttpContent content = CreateHttpContent(request);
            var task = Browser.PostAsyncAndGetJsonReponseAs<YTransactionsSearchResponse>(YodleeCommands.EXECUTE_USER_SEARCH_REQUEST, content);
            return task.Result;
        }

        /// <summary>
        /// Gets the transactions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal IEnumerable<YTransactionsSearchResponse> GetTransactions(YTransactionsSearchRequest request) {
            int startIdx = 1;
            int pageSize = 1000;
            request.SetPageSize(startIdx, pageSize);
            HttpContent content = CreateHttpContent(request);
            while (true) {
                var yTransactionsSearchResponse = Browser.PostAsyncAndGetJsonReponseAs<YTransactionsSearchResponse>(YodleeCommands.EXECUTE_USER_SEARCH_REQUEST, content).Result;
                if (yTransactionsSearchResponse.numberOfHits < 1) {
                    break;
                }

                startIdx += pageSize;
                request.SetPageSize(startIdx, pageSize);
                content = CreateHttpContent(request);

                yield return yTransactionsSearchResponse;
            }
        }

        /// <summary>
        /// Gets the site accounts.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal async Task<YSiteAccountsResponse> GetSiteAccounts(YSiteAccountsRequest request) {
            HttpContent content = CreateHttpContent(request);

            string json = await Browser.PostAsyncAndGetStringResponse(YodleeCommands.GET_SITE_ACCOUNTS, content);

            if (IsError(json)) {
                var error = JsonConvert.DeserializeObject<YError>(json);
                var response = new YSiteAccountsResponse(CollectionUtils.GetEmptyList<SiteAccountInfo>());
                response.SetError(error);
                return response;
            }

            var infos = JsonConvert.DeserializeObject<IList<SiteAccountInfo>>(json);
            return new YSiteAccountsResponse(infos);
        }

        /// <summary>
        /// Gets the site accounts as json.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal Task<string> GetSiteAccountsAsJson(YSiteAccountsRequest request) {
            HttpContent content = CreateHttpContent(request);

            return Browser.PostAsyncAndGetStringResponse(YodleeCommands.GET_SITE_ACCOUNTS, content);
        }

        /// <summary>
        /// Searches for sites.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal async Task<YSiteSearchResponse> SearchForSites(YSiteSearchRequest request) {
            HttpContent content = CreateHttpContent(request);

            string json = await Browser.PostAsyncAndGetStringResponse(YodleeCommands.SEARCH_SITE, content);

            if (IsError(json)) {
                var error = JsonConvert.DeserializeObject<YError>(json);
                var response = new YSiteSearchResponse(CollectionUtils.GetEmptyList<SiteInfo>());
                response.SetError(error);
                return response;
            }

            var infos = JsonConvert.DeserializeObject<IList<SiteInfo>>(json);
            return new YSiteSearchResponse(infos);
        }


        /// <summary>
        /// Gets the fast link token.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal async Task<YGetFastLinkTokenResponse> GetFastLinkToken(YGetFastLinkTokenRequest request) {
            HttpContent content = CreateHttpContent(request);
            string json = await Browser.PostAsyncAndGetStringResponse(YodleeCommands.GET_FASTLINK_TOKEN, content);
            return CreateResponse<YGetFastLinkTokenResponse>(json);
        }


        /// <summary>
        /// Makes the fast link.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal async Task<string> MakeFastLink(YFastLinkRequest request) {
            HttpContent content = CreateHttpContent(request);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Config.NodeUrl);

            using (var resp = await client.PostAsync(string.Empty, content)) {
                using (var cnt = resp.Content) {
                    string json = await cnt.ReadAsStringAsync();
                    return json;
                }
            }
        }

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal Task<YRegisterUserResponse> RegisterUser(YRegisterUserRequest request) {
            HttpContent content = CreateHttpContent(request);
            return Browser.PostAsyncAndGetJsonReponseAs<YRegisterUserResponse>(YodleeCommands.USER_REGISTRATION, content);
        }

        /// <summary>
        /// Creates the content of the HTTP.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private HttpContent CreateHttpContent(YRequestBase request) {
            return new FormUrlEncodedContent(request.Build());
        }

        /// <summary>
        /// Creates the response. 
        /// Used when response is not array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        private T CreateResponse<T>(string json) where T : YResponseBase, new() {
            if (json.Contains("error") || json.Contains("Error")) {
                var yError = JsonConvert.DeserializeObject<YError>(json);
                T response = new T();
                response.SetError(yError);
                return response;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Determines whether the specified json contains error response.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        private bool IsError(string json) {
            int idx = json.IndexOf("error", StringComparison.InvariantCultureIgnoreCase);
            return (idx > -1) && (idx < 30);
        }
    }
}

