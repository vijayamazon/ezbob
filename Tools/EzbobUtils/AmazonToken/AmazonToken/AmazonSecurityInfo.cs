using System.Collections.Generic;

namespace AmazonToken
{
    public class AmazonSecurityInfo
    {
        public AmazonSecurityInfo()
        {
            MarketplaceId = new List<string>();
        }

        public AmazonSecurityInfo(string merchantId, string mwsAuthToken)
        {
            MerchantId = merchantId;
            MWSAuthToken = mwsAuthToken;
        }

        public void AddMarketplace(string marketplaceId)
        {
            MarketplaceId.Add(marketplaceId);
        }

        public string MerchantId { get; set; }
        public string MWSAuthToken { get; set; }
        public List<string> MarketplaceId { get; set; }
    }
}
