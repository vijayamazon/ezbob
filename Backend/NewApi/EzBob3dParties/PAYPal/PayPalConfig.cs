using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.PAYPal
{
    /// <summary>
    /// Papal configuration
    /// </summary>
    public class PayPalConfig
    {
        /// <summary>
        /// Gets or sets the mode('live' or 'sandbox').
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the authorization URL.
        /// </summary>
        /// <value>
        /// The authorization URL.
        /// </value>
        public string AuthorizationUrl { get; set; }


        /// <summary>
        /// Converts parameters to dictionary (used by PayPal api)
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                {"mode", Mode},
                {"clientId", ClientId},
                {"clientSecret", ClientSecret}
            };
        } 
    }
}
