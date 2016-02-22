namespace EchoSignLib.Rest.Api {
    using System.Collections.Generic;
    using System.Net.Http;

    internal class EchoSignRefreshAccessTokenRequest {
        private readonly Dictionary<string, string> requestBody = new Dictionary<string, string>();

        public HttpContent BuildContent()
        {
            return new FormUrlEncodedContent(this.requestBody);
        }

        public EchoSignRefreshAccessTokenRequest() {
            SetKeyValue("grant_type", "refresh_token");
        }

        public EchoSignRefreshAccessTokenRequest WithRefreshToken(string refreshToken) {
            SetKeyValue("refresh_token", refreshToken);
            return this;
        }

        public EchoSignRefreshAccessTokenRequest WithClientId(string clientId) {
            SetKeyValue("client_id", clientId);
            return this;
        }

        public EchoSignRefreshAccessTokenRequest WithClientSecret(string clientSecret) {
            SetKeyValue("client_secret", clientSecret);
            return this;
        }

        /// <summary>
        /// Sets the key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void SetKeyValue(string key, string value)
        {
            this.requestBody[key] = value;
        }
    }
}
