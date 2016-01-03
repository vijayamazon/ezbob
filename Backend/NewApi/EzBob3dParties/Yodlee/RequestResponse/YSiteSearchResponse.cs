namespace EzBob3dParties.Yodlee.RequestResponse
{
    using System.Collections.Generic;
    using EzBob3dParties.Yodlee.Models.SiteAccount;

    /// <summary>
    /// response to YSiteSearchRequest
    /// </summary>
    class YSiteSearchResponse : YResponseBase
    {
        public YSiteSearchResponse(IList<SiteInfo> siteInfos) {
            SiteInfos = siteInfos;
        }

        /// <summary>
        /// Gets the site infos.
        /// </summary>
        /// <value>
        /// The site infos.
        /// </value>
        public IList<SiteInfo> SiteInfos { get; private set; } 
    }
}
