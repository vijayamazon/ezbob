using System.Collections.Generic;

namespace EzBobModels.Amazon {
    public class AmazonSecurityInfo {
        public string MerchantId { get; set; }
        public string MWSAuthToken { get; set; }
        public string[] MarketplaceId { get; set; }
    }
}
