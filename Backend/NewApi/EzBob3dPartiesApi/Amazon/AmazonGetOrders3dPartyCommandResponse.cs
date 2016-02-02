using System.Collections.Generic;

namespace EzBob3dPartiesApi.Amazon {
    using EzBobCommon.NSB;
    using EzBobModels.Amazon.Helpers;

    /// <summary>
    /// Response to <see cref="AmazonGetOrders3dPartyCommand"/>
    /// </summary>
    public class AmazonGetOrders3dPartyCommandResponse : CommandResponseBase {
        public IEnumerable<AmazonOrderItemAndPayments> OrderPayments { get; set; }
    }
}
