using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal
{
    using EzBobCommon.NSB;

    public class PayPalGetUserData3dPartyCommand : CommandBase
    {
        public string Token { get; set; }
    }
}
