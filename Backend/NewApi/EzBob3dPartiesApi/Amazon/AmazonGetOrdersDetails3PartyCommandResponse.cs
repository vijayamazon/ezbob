namespace EzBob3dPartiesApi.Amazon
{
    using System.Collections.Generic;
    using EzBobCommon.NSB;
    using EzBobModels.Amazon;

    /// <summary>
    /// Response to <see cref="AmazonGetOrdersDetails3PartyCommand"/>
    /// </summary>
    public class AmazonGetOrdersDetails3PartyCommandResponse : CommandResponseBase
    {
        public IDictionary<string, IList<AmazonOrderItemDetail>> OrderDetailsByOrderId { get; set; }  
    }
}
