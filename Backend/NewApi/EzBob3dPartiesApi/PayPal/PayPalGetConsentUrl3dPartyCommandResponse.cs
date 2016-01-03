using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to PayPalGetConsentUrl3dPartyCommand
    /// </summary>
    public class PayPalGetConsentUrl3dPartyCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the consent URL that is used to send user in order to get his consent to share private information with us
        /// </summary>
        /// <value>
        /// The consent URL.
        /// </value>
        public string ConsentUrl { get; set; }
    }
}
