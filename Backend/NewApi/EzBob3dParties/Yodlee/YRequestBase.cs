using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee {
    /// <summary>
    /// The base class that all yodlee requests should inherit from.
    /// </summary>
    internal abstract class YRequestBase {

        protected static readonly string CobSessionToken = "cobSessionToken";
        protected static readonly string UserSessionToken = "userSessionToken";
        protected static readonly string Fastlink2AggregationAppID = "10003600";
        protected static readonly string Fastlink2IAVAppId = "10003200";
        protected static readonly string FinAppId = "finAppId";
        protected static readonly string Rsession = "rsession";

        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        /// <summary>
        /// Inserts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void Insert(string key, string value) {
            this.parameters[key] = value;
        }

        /// <summary>
        /// Inserts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        protected void Insert(string key, bool value) {
            Insert(key, value.ToString());
        }

        /// <summary>
        /// Inserts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void Insert(string key, int value) {
            Insert(key, value.ToString());
        }

        /// <summary>
        /// Builds the request content.
        /// </summary>
        /// <returns></returns>
        internal virtual IEnumerable<KeyValuePair<string, string>> Build() {
            return this.parameters;
        }
    }
}
