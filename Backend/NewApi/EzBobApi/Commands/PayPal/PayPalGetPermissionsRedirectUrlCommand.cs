using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.PayPal
{
    using EzBobCommon.NSB;

    public class PayPalGetPermissionsRedirectUrlCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public string Callback { get; set; }
    }
}
