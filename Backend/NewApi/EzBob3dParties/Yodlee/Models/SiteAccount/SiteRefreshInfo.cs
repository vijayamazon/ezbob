using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.SiteAccount
{
    class SiteRefreshInfo
    {
        public SiteRefreshStatus siteRefreshStatus { get; set; }

        public SiteRefreshMode siteRefreshMode { get; set; }

        public int updateInitTime { get; set; }

        public int nextUpdate { get; set; }

        public int code { get; set; }
        
        public SuggestedFlow suggestedFlow { get; set; }

        public int noOfRetry { get; set; }

        public bool isMFAInputRequired { get; set; }
    }
}
