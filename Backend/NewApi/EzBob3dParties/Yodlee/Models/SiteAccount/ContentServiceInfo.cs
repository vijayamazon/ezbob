using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.SiteAccount
{
    class ContentServiceInfo
    {
        public int contentServiceId { get; set; }
        public int siteId { get; set; }
        public ContainerInfo containerInfo { get; set; }
    }
}
