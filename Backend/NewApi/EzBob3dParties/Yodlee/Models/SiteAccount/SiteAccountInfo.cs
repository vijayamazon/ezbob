using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.SiteAccount
{
    class SiteAccountInfo
    {
        public int siteAccountId { get; set; }

        public bool isCustom { get; set; }

        public int credentialsChangedTime { get; set; }

        public SiteRefreshInfo siteRefreshInfo { get; set; }

        public SiteInfo siteInfo { get; set; }

        public string created { get; set; }
        public int retryCount { get; set; }
        public bool disabled { get; set; }
        public bool isAgentError { get; set; }
        public bool isSiteError { get; set; }
        public bool isUARError { get; set; }
    }
}
