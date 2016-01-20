using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;

    public class PayPalGetTransations3dPartyCommand : CommandBase
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public DateTime UtcDateFrom { get; set; }
        public DateTime UtcDateTo { get; set; }
    }
}
