using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.PayPal
{
    using EzBobCommon.NSB;

    public class PayPalGetPermissionsRedirectUrlCommandResponse : CommandResponseBase
    {
        public string PermissionsRedirectUrl { get; set; }
    }
}
