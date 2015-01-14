using System.Collections.Generic;
using EZBob.DatabaseLib.Common;

namespace EzBob.AmazonServiceLib
{
	using System.Text;

	public class AmazonSecurityInfo : IMarketPlaceSecurityInfo
    {
        public AmazonSecurityInfo()
        {
            MarketplaceId = new List<string>();
        }

		public AmazonSecurityInfo(string merchantId, string mwsAuthToken)
            : this()
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