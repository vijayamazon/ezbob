using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.SiteAccount
{
    class SiteInfo
    {
        public int popularity { get; set; }
        public int siteId { get; set; }
        public int orgId { get; set; }

        public string defaultDisplayName { get; set; }
        public string defaultOrgDisplayName { get; set; }

        public IList<ContentServiceInfo> contentServiceInfos { get; set; }
        public IList<ContainerInfo> enabledContainers { get; set; }

        public string baseUrl { get; set; }
        public bool isHeld { get; set; }
        public bool isCustom { get; set; }
        public bool siteSearchVisibility { get; set; }
        public string loginUrl { get; set; }
        public bool isAlreadyAddedByUser { get; set; }
        public bool isOauthEnabled { get; set; }
        public int hdLogoLastModified { get; set; }
        public bool isHdLogoAvailable { get; set; }
    }
}
