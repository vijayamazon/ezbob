using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;
    using EzBobModels.PayPal;

    public class PayPalGetTransations3dPartyCommandResponse : CommandResponseBase
    {
        public IList<PayPal3dPartyTransactionItem> Transactions { get; set; } 
    }
}
