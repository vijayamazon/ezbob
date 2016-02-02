using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.ThirdParties.Amazon
{
    using EzBob3dPartiesApi.Amazon;
    using EzBobCommon.NSB;

    /// <summary>
    /// This class automatically created and registered in container by NSB
    /// </summary>
    public class AmazonGetOrderDetailsSendReceive : SendReceiveAsyncHandler<AmazonGetOrdersDetails3PartyCommandResponse>
    {
    }
}
