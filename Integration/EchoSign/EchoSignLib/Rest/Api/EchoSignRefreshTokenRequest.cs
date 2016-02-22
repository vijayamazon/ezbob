using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoSignLib.Rest.Api
{
    using System.Net.Http;

    class EchoSignRefreshTokenRequest
    {
        private Dictionary<string, string> body = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EchoSignRefreshTokenRequest() {
            SetKeyValue("grant_type", "authorization_code");
        }

        /// <summary>
        /// Sets the code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public EchoSignRefreshTokenRequest WithCode(string code) {
            SetKeyValue("code", code);
            return this;
        }

        /// <summary>
        /// Sets the client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public EchoSignRefreshTokenRequest WithClientId(string clientId) {
            SetKeyValue("client_id", clientId);
            return this;
        }

        /// <summary>
        /// Sets the client secret.
        /// </summary>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns></returns>
        public EchoSignRefreshTokenRequest WithClientSecret(string clientSecret) {
            SetKeyValue("client_secret", clientSecret);
            return this;
        }

        /// <summary>
        /// Sets the redirect URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public EchoSignRefreshTokenRequest WithRedirectUri(string uri) {
            SetKeyValue("redirect_uri", uri);
            return this;
        }

        /// <summary>
        /// Builds the content.
        /// </summary>
        /// <returns></returns>
        public HttpContent BuildContent() {
            return new FormUrlEncodedContent(this.body);
        }

        /// <summary>
        /// Sets the key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetKeyValue(string key, string value)
        {
            this.body[key] = value;
        }
    }
}
