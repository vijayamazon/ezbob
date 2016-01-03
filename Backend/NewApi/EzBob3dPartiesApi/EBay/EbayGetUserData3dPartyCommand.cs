using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.EBay
{
    using EzBobCommon.NSB;

    /// <summary>
    /// requests user ebay user data<br/>
    /// <remarks>
    /// if Token is specified - session id is ignored<br/>
    /// if Token is not specified - session id must be specified to fetch token from ebay
    /// </remarks>
    /// </summary>
    public class EbayGetUserData3dPartyCommand : CommandBase {

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public IDictionary<string, object> Payload { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }
        /// <summary>
        /// Gets or sets the get orders time from.
        /// </summary>
        /// <value>
        /// The get orders time from.
        /// </value>
        public DateTime GetOrdersTimeFrom { get; set; }
    }
}
