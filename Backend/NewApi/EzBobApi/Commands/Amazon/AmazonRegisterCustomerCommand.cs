using System.Collections.Generic;

namespace EzBobApi.Commands.Amazon
{
    using EzBobCommon.NSB;

    public class AmazonRegisterCustomerCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public string AuthorizationToken { get; set; }
        public string SellerId { get; set; }
        public IEnumerable<string> MarketplaceId { get; set; }
    }
}
