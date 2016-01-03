using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal
{
    using EzBobCommon.NSB;
    using EzBobModels.PayPal;

    /// <summary>
    /// response to PayPalGetUserData3dPartyCommand
    /// </summary>
    public class PayPalGetUserData3dPartyCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the personal information.
        /// </summary>
        /// <value>
        /// The personal information.
        /// </value>
        public PayPalUserPersonalInfo UserPersonalInfo { get; set; }
    }
}
