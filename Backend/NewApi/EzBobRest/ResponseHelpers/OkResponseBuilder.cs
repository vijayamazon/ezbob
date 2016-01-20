using System.Collections.Generic;

namespace EzBobRest.ResponseHelpers {
    using EzBobCommon.Utils;
    using Newtonsoft.Json.Linq;

    public class OkResponseBuilder {
        private readonly JObject response = new JObject();
        private static readonly string CustomerId = "CustomerId";
        private static readonly string CompanyId = "CompanyId";
        private static readonly string RedirectUrl = "RedirectUrl";
        private static readonly string SessionId = "SessionId";

        /// <summary>
        /// Adds the customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public OkResponseBuilder WithCustomerId(string customerId) {
            if (!string.IsNullOrEmpty(customerId)) {
                this.response[CustomerId] = customerId;
            }

            return this;
        }

        /// <summary>
        /// Adds the company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public OkResponseBuilder WithCompanyId(string companyId) {
            if (!string.IsNullOrEmpty(companyId)) {
                this.response[CompanyId] = companyId;
            }

            return this;
        }

        /// <summary>
        /// Adds the redirect URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public OkResponseBuilder WithRedirectUrl(string url) {
            if (!string.IsNullOrEmpty(url)) {
                this.response[RedirectUrl] = url;
            }

            return this;
        }

        /// <summary>
        /// Adds the session identifier.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns></returns>
        public OkResponseBuilder WithSessionId(string sessionId) {
            if (!string.IsNullOrEmpty(sessionId)) {
                this.response[SessionId] = sessionId;
            }

            return this;
        }

        /// <summary>
        /// Adds the key with array value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public OkResponseBuilder WithKeyArray<T>(string key, IEnumerable<T> values) where T : class {
            if (!string.IsNullOrEmpty(key) && CollectionUtils.IsEmpty(values)) {
                this.response[key] = new JArray(values);
            }

            return this;
        }

        /// <summary>
        /// Adds the key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public OkResponseBuilder WithKeyValue(string key, string val) {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val)) {
                this.response[key] = val;
            }

            return this;
        }

        public JObject BuildResponse() {
            return this.response;
        }
    }
}
