using System.Collections.Generic;

namespace EzBob3dPartiesApi.Amazon
{
    public class AmazonGetProductCategories3dPartyCommand : AmazonComandBase
    {
        public IEnumerable<string> SellerSKUs { get; set; }
        public string MarketplaceId { get; set; } 
    }
}
           