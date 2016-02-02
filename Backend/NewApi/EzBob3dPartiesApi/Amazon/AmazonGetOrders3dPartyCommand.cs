namespace EzBob3dPartiesApi.Amazon
{
    using System;
    using System.Collections.Generic;
    using EzBobCommon.NSB;

    public class AmazonGetOrders3dPartyCommand : AmazonComandBase
    {
        public IEnumerable<string> MarketplaceId { get; set; }
        public DateTime DateFrom { get; set; }
    }
}
