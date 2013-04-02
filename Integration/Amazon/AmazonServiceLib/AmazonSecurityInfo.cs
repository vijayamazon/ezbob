using System.Collections.Generic;
using EZBob.DatabaseLib.Common;

namespace EzBob.AmazonServiceLib
{
    public class AmazonSecurityInfo : IMarketPlaceSecurityInfo
    {
        public AmazonSecurityInfo()
        {
            MarketplaceId = new List<string>();
        }

        public AmazonSecurityInfo(string merchantId)
            : this()
        {
            MerchantId = merchantId;

        }

        public void AddMarketplace(string marketplaceId)
        {
            MarketplaceId.Add(marketplaceId);
        }

        public string MerchantId { get; set; }
        public List<string> MarketplaceId { get; set; }

    }
}