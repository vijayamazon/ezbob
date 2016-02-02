using System.Collections.Generic;

namespace EzBob3dPartiesApi.Amazon
{
    using EzBobCommon.NSB;

    public class AmazonGetOrdersDetails3PartyCommand : AmazonComandBase
    {
        public IEnumerable<string> OrdersIds { get; set; }
    }
}
