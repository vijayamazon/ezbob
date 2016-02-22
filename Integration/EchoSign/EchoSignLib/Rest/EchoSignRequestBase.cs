using System.Collections.Generic;

namespace EchoSignLib.Rest
{
    using System.Net.Http;

    internal abstract class EchoSignRequestBase
    {
        private readonly Dictionary<string, string> requestBody = new Dictionary<string, string>();

        public HttpContent BuildContent() {
            return new FormUrlEncodedContent(this.requestBody);
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
