using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Asks for ConsentUrl
    /// </summary>
    public class PayPalGetConsentUrl3dPartyCommand : CommandBase {
        /// <summary>
        /// Gets or sets the redirect URL where we want PayPal to send us authorization token
        /// </summary>
        /// <value>
        /// The redirect URL.
        /// </value>
        public string RedirectUrl { get; set; }
    }
}
