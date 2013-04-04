using EZBob.DatabaseLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EKM
{
    class EkmSecurityInfo : IMarketPlaceSecurityInfo
    {
        public EkmSecurityInfo()
        {
            MarketplaceId = new List<string>();
        }

        public void AddMarketplace(string marketplaceId)
        {
            MarketplaceId.Add(marketplaceId);
        }

          public List<string> MarketplaceId { get; set; }
    }
}
