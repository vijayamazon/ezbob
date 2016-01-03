using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Ebay
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to GetEbayLoginUrlCommand
    /// </summary>
    public class EbayGetLoginUrlCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the ebay login URL.
        /// </summary>
        /// <value>
        /// The ebay login URL.
        /// </value>
        public string EbayLoginUrl { get; set; }
    }
}
